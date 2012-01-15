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

            logger.Log("NewSnapper created", Category.Debug, Priority.None);
        }

        public ITemporarySnap TemporarySnap(NewPrimitive newPrimitive)
        {
            return new TemporarySnap(sessionData, snappersManager, primitivesReaderWriterFactory, eventAggregator, newPrimitive, constrainedOptimizer);
        }

        public void Snap()
        {
            if (sessionData.SelectedNewPrimitives.Count == 1)
            {
                // initialize our snapped primitive
                var newPrimitive = sessionData.SelectedNewPrimitives.First();
                var snappedPrimitive = snappersManager.Create(newPrimitive);
                snappedPrimitive.UpdateFeatureCurves();

                //MessageBox.Show("So far so good");
                // update session data
                sessionData.SnappedPrimitives.Add(snappedPrimitive);
                sessionData.NewPrimitives.Remove(newPrimitive);
                sessionData.FeatureCurves.AddRange(snappedPrimitive.FeatureCurves);

                OptimizeAll();

                // update annotations with the inferred ones and optimize again
                var annotations = annotationInference.InferAnnotations(newPrimitive, snappedPrimitive);
                if (annotations.Any())
                {
                    sessionData.Annotations.AddRange(annotations);
                    OptimizeAll();
                }
            }
            else
                OptimizeAll();

            eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
        }

        public void Recalculate()
        {
            if (sessionData.SnappedPrimitives.Count > 0)
            {
                OptimizeAll();
                eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
            }
        }

        private void OptimizeAll()
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

            var vars = primitivesWriter.GetVariables();
            var vals = primitivesWriter.GetValues();

            var finalObjective = TermUtils.SafeSum(objectives);
            var optimum = constrainedOptimizer.Minimize(
                finalObjective, constraints, vars, vals).Last();

            // update primitives from the optimal values
            primitivesReaderWriterFactory.CreateReader().Read(optimum, sessionData.SnappedPrimitives);
            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
                snappedPrimitive.UpdateFeatureCurves();
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
    }
}
