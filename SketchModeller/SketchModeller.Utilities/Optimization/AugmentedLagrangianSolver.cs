using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// Solves a constraint optimization problem using the AugmentedLagrangian algorithm.
    /// </summary>
    public class AugmentedLagrangianSolver
    {
        private readonly IConvergenceTest convergenceTest;
        private readonly IAugmentedLagrangianIterations iterations;

        /// <summary>
        /// Convergence test to use
        /// </summary>
        /// <param name="convergenceTest">An interface to the convergence testing class.</param>
        /// <param name="iterations">A class that provides the iterations of the algorithm.</param>
        public AugmentedLagrangianSolver(IConvergenceTest convergenceTest, IAugmentedLagrangianIterations iterations)
        {
            this.convergenceTest = convergenceTest;
            this.iterations = iterations;
        }

        /// <summary>
        /// Solves a constrained optimization problem.
        /// </summary>
        /// <param name="objective">The objective function</param>
        /// <param name="constraints">The constraints</param>
        /// <param name="variables">The variables</param>
        /// <param name="startPoint">The initial guess for the minimizer.</param>
        /// <returns>The optimal value computed for this optimization problem.</returns>
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
