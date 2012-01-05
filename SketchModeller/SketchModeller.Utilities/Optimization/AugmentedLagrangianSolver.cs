using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    class AugmentedLagrangianSolver
    {
        private readonly IConvergenceTest convergenceTest;
        private readonly IAugmentedLagrangianIterations iterations;

        public AugmentedLagrangianSolver(IConvergenceTest convergenceTest, IAugmentedLagrangianIterations iterations)
        {
            this.convergenceTest = convergenceTest;
            this.iterations = iterations;
        }

        public double[] Solve(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint)
        {
            convergenceTest.Reset();
            foreach (var iterationResult in iterations.Start(objective, constraints, variables, startPoint))
            {
                convergenceTest.Update(iterationResult);
                if (convergenceTest.HasConverged)
                    return iterationResult.Values;
            }
            throw new InvalidOperationException("Iterations should run indefinately until convergence. We should not reach this point.");
        }
    }
}
