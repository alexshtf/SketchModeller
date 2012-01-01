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

namespace SketchModeller.Modelling.Services.Snap
{
    class TemporarySnap : ITemporarySnap
    {
        private readonly SessionData sessionData;
        private readonly SnappersManager snappersManager;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly SnappedPrimitive snappedPrimitive;

        private Task optimizationTask;
        private CancellationToken cancellationToken;
        private bool shouldOptimizeAgain;

        public TemporarySnap(SessionData sessionData,
                             SnappersManager snappersManager,
                             PrimitivesReaderWriterFactory primitivesReaderWriterFactory,
                             IEventAggregator eventAggregator,
                             NewPrimitive newPrimitive)
        {
            this.sessionData = sessionData;
            this.snappersManager = snappersManager;
            this.primitivesReaderWriterFactory = primitivesReaderWriterFactory;
            this.eventAggregator = eventAggregator;

            snappedPrimitive = snappersManager.Create(newPrimitive);
            sessionData.SnappedPrimitives.Add(snappedPrimitive);
        }

        public void Update()
        {
            if (optimizationTask != null)
            {
                shouldOptimizeAgain = true;
                return;
            }

            var emptyCurvesToAnnotations = new Dictionary<FeatureCurve, ISet<Annotation>>();

            var objectivesAndConstraints = snappersManager.Reconstruct(snappedPrimitive, emptyCurvesToAnnotations);
            var objective = objectivesAndConstraints.Item1;
            var constraints = objectivesAndConstraints.Item2;

            var primitivesWriter = primitivesReaderWriterFactory.CreateWriter();
            primitivesWriter.Write(snappedPrimitive);

            var vars = primitivesWriter.GetVariables();
            var vals = primitivesWriter.GetValues();

            optimizationTask = Task.Factory.StartNew<double[]>(
                () => ALBFGSOptimizer.Minimize(objective, constraints, vars, vals, mu: 10, tolerance: 1E-5))
                .ContinueWith(task =>
                {
                    var optimum = task.Result;

                    // update primitives from the optimal values
                    primitivesReaderWriterFactory.CreateReader().Read(optimum, snappedPrimitive);
                    
                    // update the task managment fields.
                    optimizationTask = null;
                    if (shouldOptimizeAgain)
                    {
                        shouldOptimizeAgain = false;
                        Update();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Dispose()
        {
            if (optimizationTask != null)
                optimizationTask.Wait();

            sessionData.SnappedPrimitives.Remove(snappedPrimitive);
        }
    }
}
