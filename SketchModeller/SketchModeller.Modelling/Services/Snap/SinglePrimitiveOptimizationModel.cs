using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Shared;
using AutoDiff;

using TermUtils = SketchModeller.Utilities.TermUtils;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics;
using SketchModeller.Utilities.Optimization;

namespace SketchModeller.Modelling.Services.Snap
{
    class SinglePrimitiveOptimizationModel : IOptimizationModel
    {
        private readonly IOptimizationModel wrappedModel;
        private readonly PrimitivesReaderWriterFactory primitivesReaderWriterFactory;
        private readonly SnappedPrimitive specificPrimitive;

        public SinglePrimitiveOptimizationModel(
            IOptimizationModel wrappedModel,
            PrimitivesReaderWriterFactory primitivesReaderWriterFactory,
            SnappedPrimitive specificPrimitive)
        {
            this.wrappedModel = wrappedModel;
            this.primitivesReaderWriterFactory = primitivesReaderWriterFactory;
            this.specificPrimitive = specificPrimitive;
        }

        public OptimizationProblem CreateProblem()
        {
            // extract wrapped optimization problem
            var wrappedProblem = wrappedModel.CreateProblem();
            var variables = wrappedProblem.Variables;
            var values = wrappedProblem.InitialValue;
            var objective = wrappedProblem.Objective;
            var constraints = wrappedProblem.Constraints;

            // now we are going to transform the objective and the constraints
            var specificPrimitiveWriter = primitivesReaderWriterFactory.CreateWriter();
            specificPrimitiveWriter.Write(specificPrimitive);

            var specificVariables = specificPrimitiveWriter.GetVariables();
            var specificValues = specificPrimitiveWriter.GetValues();

            var substitutionDictionary = new Dictionary<Variable, double>();
            var specificVariablesSet = new HashSet<Variable>(specificVariables);
            for (int i = 0; i < variables.Length; i++)
            {
                if (!specificVariablesSet.Contains(variables[i]))
                    substitutionDictionary.Add(variables[i], values[i]);
            }

            var newObjective = objective.Substitute(substitutionDictionary);
            var newConstraints = from constraint in constraints
                                 let substituted = constraint.Substitute(substitutionDictionary)
                                 where !(substituted is Zero || substituted is Constant)
                                 select substituted;

            return new OptimizationProblem
            {
                Objective = newObjective,
                Constraints = newConstraints.ToArray(),
                Variables = specificVariables,
                InitialValue = specificValues
            };
        }

        public void UpdateSolution(double[] solution)
        {
            primitivesReaderWriterFactory.CreateReader().Read(solution, specificPrimitive);
            specificPrimitive.UpdateFeatureCurves();
        }
    }
}
