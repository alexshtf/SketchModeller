using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using AutoDiff;
using SketchModeller.Utilities.Optimization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SketchModeller.Modelling.Services.ConstrainedOptimizer
{
    class ConstrainedOptimizerService : IConstrainedOptimizer
    {
        private readonly AugmentedLagrangianSolver augmentedLagrangianSolver;

        public ConstrainedOptimizerService()
        {
            var lagrangianCompiler = new LagrangianCompiler();
            var unconstrainedOptimizer = new LBFGSOptimizer();
            //var unconstrainedOptimizer = new ConjugateGradientOptimizer();
            var iterations = new AugmentedLagrangianIterations(
                unconstrainedOptimizer, 
                lagrangianCompiler, 
                startConstraintsPenalty: 10,
                constraintsPenaltyMax: 1E3,
                maxConstraintsNormLowerBound: 1E-8,
                lagrangianGradientNormLowerBound: 1E-6);

            var convergenceTest = new ConstraintsNormWithGradientNormConvergenceTest(
                constraintsNormMax: 1E-8,
                lagrangianGradientNormMax: 2E-6,
                maxIterations: 150);
            augmentedLagrangianSolver = new AugmentedLagrangianSolver(convergenceTest, iterations);
        }

        public double[] Minimize(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            //DebugSave(objective, constraints, vars, startPoint);
            return augmentedLagrangianSolver.Solve(objective, constraints, vars, startPoint);
        }

        /*
        private void DebugSave(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            var time = DateTime.Now;
            string fileName = "aaaopt" + time.Ticks + ".opt";
            using (var stream = File.Create(fileName))
            {
                var formatter = new BinaryFormatter();
                var tuple = Tuple.Create(objective, constraints.ToArray(), vars, startPoint);
                formatter.Serialize(stream, tuple);
            }
        }*/
    }
}
