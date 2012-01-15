using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using SketchModeller.Utilities;

using TermUtils = SketchModeller.Utilities.TermUtils;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using SketchModeller.Infrastructure.Events;

namespace SketchModeller.Modelling.Services.Snap
{
    class TemporarySnap : ITemporarySnap
    {
        private readonly SessionData sessionData;
        private readonly SnappersManager snappersManager;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly NewPrimitive newPrimitive;
        private readonly IConstrainedOptimizer optimizer;

        private SnappedPrimitive oldSnappedPrimitive;
        private SnappedPrimitive snappedPrimitive;
        private Task optimizationTask;
        private volatile bool shouldOptimizeAgain;
        private bool disposed;

        public TemporarySnap(SessionData sessionData,
                             SnappersManager snappersManager,
                             PrimitivesReaderWriterFactory primitivesReaderWriterFactory,
                             IEventAggregator eventAggregator,
                             NewPrimitive newPrimitive,
                             IConstrainedOptimizer optimizer)
        {
            this.sessionData = sessionData;
            this.snappersManager = snappersManager;
            this.primitivesReaderWriterFactory = primitivesReaderWriterFactory;
            this.eventAggregator = eventAggregator;
            this.newPrimitive = newPrimitive;
            this.optimizer = optimizer;
        }

        public void Update()
        {
            if (disposed)
                return;

            DoUpdate();
        }

        private void DoUpdate()
        {
            if (optimizationTask != null)
            {
                shouldOptimizeAgain = true;
                return;
            }

            oldSnappedPrimitive = snappedPrimitive;
            snappedPrimitive = snappersManager.Create(newPrimitive);
            snappedPrimitive.UpdateFeatureCurves();

            var emptyCurvesToAnnotations = new Dictionary<FeatureCurve, ISet<Annotation>>();

            var objectivesAndConstraints = snappersManager.Reconstruct(snappedPrimitive, emptyCurvesToAnnotations);
            var objective = objectivesAndConstraints.Item1;
            var constraints = objectivesAndConstraints.Item2;

            var primitivesWriter = primitivesReaderWriterFactory.CreateWriter();
            primitivesWriter.Write(snappedPrimitive);

            var vars = primitivesWriter.GetVariables();
            var vals = primitivesWriter.GetValues();

            optimizationTask = Task.Factory.StartNew<double[]>(
                _ => optimizer.Minimize(objective, constraints, vars, vals).Last(), TaskScheduler.Default)
                .ContinueWith(task =>
                {
                    if (disposed)
                        return;

                    var optimum = task.Result;

                    // update primitives from the optimal values
                    primitivesReaderWriterFactory.CreateReader().Read(optimum, snappedPrimitive);
                    snappedPrimitive.UpdateFeatureCurves();

                    // update the task managment fields.
                    sessionData.SnappedPrimitives.Remove(oldSnappedPrimitive);
                    optimizationTask = null;
                    if (shouldOptimizeAgain)
                    {
                        shouldOptimizeAgain = false;
                        DoUpdate();
                    }
                    else
                    {
                        sessionData.SnappedPrimitives.Add(snappedPrimitive);
                        eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Dispose()
        {
            sessionData.SnappedPrimitives.Remove(oldSnappedPrimitive);
            sessionData.SnappedPrimitives.Remove(snappedPrimitive);
            disposed = true;
        }
    }
}
