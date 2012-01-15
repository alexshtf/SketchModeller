using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Logging;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using AutoDiff;
using SketchModeller.Utilities;
using Utils;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using TermUtils = SketchModeller.Utilities.TermUtils;
using Enumerable = System.Linq.Enumerable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Input;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly UiState uiState;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;
        private readonly SnappersManager snappersManager;
        private readonly IAnnotationInference annotationInference;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;
        private readonly IAnnotationConstraintsExtractor annotationConstraintsExtractor;
        private readonly IConstrainedOptimizer constrainedOptimizer;

        [InjectionConstructor]
        public Snapper(
            SessionData sessionData,
            UiState uiState,
            ILoggerFacade logger,
            IUnityContainer container,
            IEventAggregator eventAggregator,
            IAnnotationInference annotationInference,
            IConstrainedOptimizer constrainedOptimizer)
        {
            this.sessionData = sessionData;
            this.uiState = uiState;
            this.logger = logger;
            this.container = container;
            this.eventAggregator = eventAggregator;
            this.annotationInference = annotationInference;
            this.constrainedOptimizer = constrainedOptimizer;

            this.primitivesReaderWriterFactory = new PrimitivesReaderWriterFactory();
            this.annotationConstraintsExtractor = new AnnotationConstraintsExtractor();

            snappersManager = new SnappersManager(uiState, sessionData);
            snappersManager.RegisterSnapper(new ConeSnapper());
            snappersManager.RegisterSnapper(new CylinderSnapper());
            snappersManager.RegisterSnapper(new SphereSnapper());
            snappersManager.RegisterSnapper(new SgcSnapper());
            snappersManager.RegisterSnapper(new BgcSnapper());

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
                var scheduler = new DispatcherScheduler(dispatcher);

                return from tuple in Observable.Start<Tuple<NewPrimitive, SnappedPrimitive>>(ConvertToSnapped, scheduler)
                       let newPrimitive = tuple.Item1
                       let snappedPrimitive = tuple.Item2
                       from unit1 in OptimizeAllAsync(scheduler)
                       let annotations = annotationInference.InferAnnotations(newPrimitive, snappedPrimitive)
                       from unit2 in annotations.Any() ? OptimizeWithAnnotationsAsync(scheduler, annotations) 
                                                       : Observable.Empty<Unit>()
                       select unit2;
            }
            else
                return RecalculateAsync();
        }

        private void OnGlobalShortcut(KeyEventArgs e)
        {
        }

        private IObservable<Unit> OptimizeWithAnnotationsAsync(DispatcherScheduler scheduler, IEnumerable<Annotation> annotations)
        {
            return from unit1 in Observable.Start(() => sessionData.Annotations.AddRange(annotations), scheduler)
                   from unit2 in OptimizeAllAsync(scheduler)
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

            return from unit1 in OptimizeAllAsync(scheduler)
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

        private IObservable<Unit> OptimizeAllAsync(DispatcherScheduler scheduler)
        {
            return
                from optimizationProblem in Observable.Start<OptimizationProblem>(GetOptimizationProblem, scheduler)
                from unit in IterativelySolveAsync(optimizationProblem, scheduler)
                select unit;
        }

        private IObservable<Unit> IterativelySolveAsync(OptimizationProblem problem, DispatcherScheduler scheduler)
        {
            var optimizationSequence = constrainedOptimizer.Minimize(
                problem.Objective, problem.Constraints, problem.Variables, problem.InitialValue);

            var optimumObservations =
                optimizationSequence.ToObservable(Scheduler.ThreadPool);

            var solutionSequence = from optimumBuffer in optimumObservations.Buffer(TimeSpan.FromSeconds(1)) // take optimization result every 1 second
                                   where optimumBuffer.Any()
                                   let optimum = optimumBuffer.Last()
                                   from unit in Observable.Start(() => UpdateSolution(optimum), scheduler)
                                   select unit;

            return solutionSequence.TakeLast(1);
        }

        private OptimizationProblem GetOptimizationProblem()
        {
            var curvesToAnnotations = GetCurvesToAnnotationsMapping();

            // get objectives and constraints for primitives
            var constraints = new List<Term>();
            var objectives = new List<Term>();
            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
            {
                var objectiveAndConstraints = snappersManager.Reconstruct(snappedPrimitive, curvesToAnnotations);
                objectives.Add(objectiveAndConstraints.Item1);
                constraints.AddRange(objectiveAndConstraints.Item2);
            }

            // add constraints extracted from the annotations
            var annotationConstraints = from annotation in sessionData.Annotations
                                        from constraint in annotationConstraintsExtractor.GetConstraints(annotation)
                                        select constraint;
            constraints.AddRange(annotationConstraints);

            // perform the optimization.
            var primitivesWriter = primitivesReaderWriterFactory.CreateWriter();
            primitivesWriter.Write(sessionData.SnappedPrimitives);

            var variables = primitivesWriter.GetVariables();
            var values = primitivesWriter.GetValues();

            var finalObjective = TermUtils.SafeSum(objectives);

            return new OptimizationProblem
            {
                Objective = finalObjective,
                Constraints = constraints.ToArray(),
                Variables = variables,
                InitialValue = values,
            };
        }

        private void UpdateSolution(double[] optimum)
        {
            primitivesReaderWriterFactory.CreateReader().Read(optimum, sessionData.SnappedPrimitives);
            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
                snappedPrimitive.UpdateFeatureCurves();
            NotifySnapComplete();
        }

        private Dictionary<FeatureCurve, ISet<Annotation>> GetCurvesToAnnotationsMapping()
        {
            var curvesToAnnotations = new Dictionary<FeatureCurve, ISet<Annotation>>();
            foreach (var fc in sessionData.FeatureCurves)
                curvesToAnnotations[fc] = new HashSet<Annotation>();

            foreach (var annotation in sessionData.Annotations)
            {
                IEnumerable<FeatureCurve> curves = annotation.Elements;
                Debug.Assert(curves != null);
                foreach (var fc in curves)
                    curvesToAnnotations[fc].Add(annotation);
            }
            return curvesToAnnotations;
        }

        class OptimizationProblem
        {
            public Term Objective { get; set; }
            public Term[] Constraints { get; set; }
            public double[] InitialValue { get; set; }
            public Variable[] Variables { get; set; }
        }
    }
}
