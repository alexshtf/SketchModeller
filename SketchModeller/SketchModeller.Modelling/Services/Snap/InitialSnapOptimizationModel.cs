using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Services.Snap
{
    class InitialSnapOptimizationModel : IOptimizationModel
    {
        private readonly SessionData sessionData;
        private readonly SnappedPrimitive snappedPrimitive;
        private readonly SnappersManager snappersManager;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;

        public InitialSnapOptimizationModel(SessionData sessionData, SnappedPrimitive snappedPrimitive, SnappersManager snappersManager, PrimitivesReaderWriterFactory primitivesReaderWriterFactory)
        {
            this.sessionData = sessionData;
            this.snappedPrimitive = snappedPrimitive;
            this.snappersManager = snappersManager;
            this.primitivesReaderWriterFactory = primitivesReaderWriterFactory;
        }

        public OptimizationProblem CreateProblem()
        {
            // create an empty mapping of curves to annotations, as we have no annotations
            var curvesToAnnotations = new Dictionary<FeatureCurve, ISet<Annotation>>();
            foreach (var fc in sessionData.FeatureCurves)
                curvesToAnnotations[fc] = new HashSet<Annotation>();

            // now we are going to transform the objective and the constraints
            var primitiveWriter = primitivesReaderWriterFactory.CreateWriter();
            primitiveWriter.Write(snappedPrimitive);

            // now we construct all we need for the optimization problem
            var variables = primitiveWriter.GetVariables();
            var initialValues = primitiveWriter.GetValues();
            var objectiveWithConstraints = snappersManager.Reconstruct(snappedPrimitive, curvesToAnnotations);

            return new OptimizationProblem
            {
                Objective = objectiveWithConstraints.Item1,
                Constraints = objectiveWithConstraints.Item2,
                Variables = variables,
                InitialValue = initialValues,
            };
        }

        public void UpdateSolution(double[] solution)
        {
            primitivesReaderWriterFactory.CreateReader().Read(solution, snappedPrimitive);
            snappedPrimitive.UpdateFeatureCurves();
        }
    }
}
