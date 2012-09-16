using System.Diagnostics;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using Utils;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly IEventAggregator eventAggregator;
        private readonly IAnnotationInference annotationInference;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;
        private readonly IConstrainedOptimizer constrainedOptimizer;
        private readonly SnappersManager snappersManager;
        private readonly IOptimizationModel wholeShapeOptimizationModel;

        private int stopOptimization;

        [InjectionConstructor]
        public Snapper(
            SessionData sessionData,
            UiState uiState,
            ILoggerFacade logger,
            IEventAggregator eventAggregator,
            IAnnotationInference annotationInference,
            IConstrainedOptimizer constrainedOptimizer)
        {
            this.sessionData = sessionData;
            this.eventAggregator = eventAggregator;
            this.annotationInference = annotationInference;
            this.constrainedOptimizer = constrainedOptimizer;

            primitivesReaderWriterFactory = new PrimitivesReaderWriterFactory();
            var annotationConstraintsExtractor = new AnnotationConstraintsExtractor();

            snappersManager = new SnappersManager(uiState, sessionData);
            snappersManager.RegisterSnapper(new ConeSnapper());
            snappersManager.RegisterSnapper(new CylinderSnapper());
            snappersManager.RegisterSnapper(new SphereSnapper());
            snappersManager.RegisterSnapper(new SgcSnapper());
            snappersManager.RegisterSnapper(new BgcSnapper());
            snappersManager.RegisterSnapper(new CuboidSnapper());
            
            wholeShapeOptimizationModel = new WholeShapeOptimizationModel(sessionData, snappersManager, annotationConstraintsExtractor, primitivesReaderWriterFactory);

            eventAggregator.GetEvent<GlobalShortcutEvent>().Subscribe(OnGlobalShortcut);

            logger.Log("NewSnapper created", Category.Debug, Priority.None);
        }

        public ITemporarySnap TemporarySnap(NewPrimitive newPrimitive)
        {
            return new TemporarySnap(sessionData, snappersManager, primitivesReaderWriterFactory, eventAggregator, newPrimitive, constrainedOptimizer);
        }

        public IObservable<Unit> SnapAsync()
        {
            if (sessionData.SelectedNewPrimitives.Count == 1)
            {
                var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                Debug.Assert(dispatcher != null, "dispatcher != null");
                var scheduler = new DispatcherScheduler(dispatcher);

                return from tuple in Observable.Start<Tuple<NewPrimitive, SnappedPrimitive>>(ConvertToSnapped, scheduler)
                       let newPrimitive = tuple.Item1
                       let snappedPrimitive = tuple.Item2
                       let initialSnapOptimizationModel = new InitialSnapOptimizationModel(sessionData, snappedPrimitive, snappersManager, primitivesReaderWriterFactory)
                       from unit1 in OptimizeAsync(scheduler, initialSnapOptimizationModel)
                       let annotations = annotationInference.InferAnnotations(newPrimitive, snappedPrimitive)
                       let optimizationModel = wholeShapeOptimizationModel
                       from unit2 in annotations.Any() ? OptimizeWithAnnotationsAsync(scheduler, annotations, optimizationModel)
                                                       : Observable.Empty<Unit>()
                       select unit2;
            }
            else
                return RecalculateAsync();
        }

        private void OnGlobalShortcut(KeyEventArgs e)
        {
            if (e.Key == Key.C && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                Interlocked.Exchange(ref stopOptimization, 1);
        }

        private IObservable<Unit> OptimizeWithAnnotationsAsync(DispatcherScheduler scheduler, IEnumerable<Annotation> annotations, IOptimizationModel optimizationModel)
        {
            return from unit1 in Observable.Start(() => sessionData.Annotations.AddRange(annotations), scheduler)
                   from unit2 in OptimizeAsync(scheduler, optimizationModel)
                   select unit2;
        }

        private Tuple<NewPrimitive, SnappedPrimitive> ConvertToSnapped()
        {
            // initialize our snapped primitive
            var newPrimitive = sessionData.SelectedNewPrimitives.First();
            var snappedPrimitive = snappersManager.Create(newPrimitive);
            snappedPrimitive.UpdateFeatureCurves();

            // update session data
            sessionData.SnappedPrimitives.Add(snappedPrimitive);
            sessionData.NewPrimitives.Remove(newPrimitive);
            sessionData.FeatureCurves.AddRange(snappedPrimitive.FeatureCurves);

            return Tuple.Create(newPrimitive, snappedPrimitive);
        }

        public IObservable<Unit> RecalculateAsync()
        {
            var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            var scheduler = new DispatcherScheduler(dispatcher);

            return from unit1 in OptimizeAsync(scheduler, wholeShapeOptimizationModel)
                   from unit2 in NotifySnapCompleteAsync(scheduler)
                   select unit2;
        }

        private IObservable<Unit> NotifySnapCompleteAsync(DispatcherScheduler scheduler)
        {
            return Observable.Start(NotifySnapComplete, scheduler);
        }

        private void NotifySnapComplete()
        {
            eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
        }

        private IObservable<Unit> OptimizeAsync(DispatcherScheduler scheduler, IOptimizationModel optimizationModel)
        {
            return
                from optimizationProblem in Observable.Start<OptimizationProblem>(optimizationModel.CreateProblem, scheduler)
                from unit in IterativelySolveAndUpdateAsync(optimizationProblem, optimizationModel, scheduler)
                select unit;
        }

        private IObservable<Unit> IterativelySolveAndUpdateAsync(OptimizationProblem problem, IOptimizationModel optimizationModel, DispatcherScheduler scheduler)
        {
            var optimizationSequence = Minimize(problem);

            var optimumObservations =
                optimizationSequence.ToObservable(Scheduler.ThreadPool);

            var solutionSequence = from optimumBuffer in optimumObservations.Buffer(TimeSpan.FromSeconds(1)) // take optimization result every 1 second
                                   where optimumBuffer.Any()
                                   let optimum = optimumBuffer.Last()
                                   from unit in Observable.Start(() =>
                                       {
                                           optimizationModel.UpdateSolution(optimum);
                                           NotifySnapComplete();
                                       }, scheduler)
                                   select unit;

            return solutionSequence.TakeLast(1);
        }

        private IEnumerable<double[]> Minimize(OptimizationProblem problem)
        {
            foreach (var optimum in constrainedOptimizer.Minimize(problem.Objective, problem.Constraints, problem.Variables, problem.InitialValue))
            {
                yield return optimum;
             
                var shouldStop = Interlocked.CompareExchange(ref stopOptimization, 0, 1);
                if (shouldStop == 1)
                    yield break;
            }
        }
    }
}
