﻿using System;
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
            var lagrangianCompiler = new LagrangianCompiler();
            var unconstrainedOptimizer = new LBFGSOptimizer();
            var iterations = new AugmentedLagrangianIterations(unconstrainedOptimizer, lagrangianCompiler, startConstraintsPenalty: 10);

            var convergenceTest = new ConstraintsNormWithGradientNormConvergenceTest(
                constraintsNormMax: 1E-4,
                lagrangianGradientNormMax: 1E-6);
            augmentedLagrangianSolver = new AugmentedLagrangianSolver(convergenceTest, iterations);
        }

        public double[] Minimize(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            return augmentedLagrangianSolver.Solve(objective, constraints, vars, startPoint);
        }
    }
}
