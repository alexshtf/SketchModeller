using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using AutoDiff;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics;

using TermUtils = SketchModeller.Utilities.TermUtils;
using Microsoft.Practices.Prism.Events;

namespace SketchModeller.Modelling.Services.Snap
{
    class WholeShapeOptimizationModel : IOptimizationModel
    {
        private readonly SnappersManager snappersManager;
        private readonly SessionData sessionData;
        private readonly IAnnotationConstraintsExtractor annotationConstraintsExtractor;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;

        public WholeShapeOptimizationModel(
            SessionData sessionData, 
            SnappersManager snappersManager, 
            IAnnotationConstraintsExtractor annotationConstraintsExtractor, 
            PrimitivesReaderWriterFactory primitivesReaderWriterFactory)
        {
            this.sessionData = sessionData;
            this.snappersManager = snappersManager;
            this.annotationConstraintsExtractor = annotationConstraintsExtractor;
            this.primitivesReaderWriterFactory = primitivesReaderWriterFactory;
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

        public OptimizationProblem CreateProblem()
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

        public void UpdateSolution(double[] solution)
        {
            primitivesReaderWriterFactory.CreateReader().Read(solution, sessionData.SnappedPrimitives);
            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
                snappedPrimitive.UpdateFeatureCurves();
        }
    }
}
