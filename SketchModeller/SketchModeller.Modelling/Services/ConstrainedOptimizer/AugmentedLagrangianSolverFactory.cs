using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Utilities.Optimization;

namespace SketchModeller.Modelling.Services.ConstrainedOptimizer
{
    class AugmentedLagrangianSolverFactory
    {
        public AugmentedLagrangianSolverFactory()
        {
        }

        public IConstrainedSolver Create()
        {
            var lagrangianCompiler = new LagrangianCompiler();
            var unconstrainedOptimizer = new LBFGSOptimizer(30);
            //var unconstrainedOptimizer = new ConjugateGradientOptimizer();
            var iterations = new AugmentedLagrangianIterations(
                unconstrainedOptimizer,
                lagrangianCompiler,
                startConstraintsPenalty: 10,
                constraintsPenaltyMax: 1E3,
                maxConstraintsNormLowerBound: 1E-8,
                lagrangianGradientNormLowerBound: 4E-7);

            var convergenceTest = new ConstraintsNormWithGradientNormConvergenceTest(
                constraintsNormMax: 1E-8,
                lagrangianGradientNormMax: 2E-6,
                maxIterations: 1000);
            var solver = new AugmentedLagrangianSolver(convergenceTest, iterations);

            return solver;
        }
    }
}
