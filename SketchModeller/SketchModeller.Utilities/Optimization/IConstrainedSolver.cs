using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// Interface to a solver of a constrained optimization problem.
    /// </summary>
    public interface IConstrainedSolver
    {
        /// <summary>
        /// Solves a constrained optimization problem.
        /// </summary>
        /// <param name="objective">The objective function</param>
        /// <param name="constraints">The constraints</param>
        /// <param name="variables">The variables</param>
        /// <param name="startPoint">The initial guess for the minimizer.</param>
        /// <returns>A sequence of approximations for the optimal values of the algorithm. The sequence terminates when 
        /// the solver decided it has converged.</returns>
        IEnumerable<double[]> Solve(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint);
    }
}
