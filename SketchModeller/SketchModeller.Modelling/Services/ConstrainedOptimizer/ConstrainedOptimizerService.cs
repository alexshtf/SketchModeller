using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using AutoDiff;
using SketchModeller.Utilities.Optimization;

namespace SketchModeller.Modelling.Services.ConstrainedOptimizer
{
    class ConstrainedOptimizerService : IConstrainedOptimizer
    {
        private readonly AugmentedLagrangianSolver augmentedLagrangianSolver;

        public ConstrainedOptimizerService()
        {
            const double convergenceConstraintNormMax = 1E-8;

            var lagrangianCompiler = new LagrangianCompiler();
            var unconstrainedOptimizer = new LBFGSOptimizer();
            var iterations = new AugmentedLagrangianIterations(
                unconstrainedOptimizer, 
                lagrangianCompiler, 
                startConstraintsPenalty: 10,
                constraintsPenaltyMax: 1E10,
                maxConstraintsNormLowerBound: convergenceConstraintNormMax);

            var convergenceTest = new ConstraintsNormWithGradientNormConvergenceTest(
                constraintsNormMax: convergenceConstraintNormMax,
                lagrangianGradientNormMax: 1E-4);
            augmentedLagrangianSolver = new AugmentedLagrangianSolver(convergenceTest, iterations);
        }

        public double[] Minimize(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            return augmentedLagrangianSolver.Solve(objective, constraints, vars, startPoint);
        }
    }
}
