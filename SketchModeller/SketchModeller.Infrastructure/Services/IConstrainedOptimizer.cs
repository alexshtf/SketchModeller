using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Infrastructure.Services
{
    /// <summary>
    /// Performs optimization with equality constraints
    /// </summary>
    public interface IConstrainedOptimizer
    {
        /// <summary>
        /// Performs constrained minimization
        /// </summary>
        /// <param name="objective">The objective function term</param>
        /// <param name="constraints">The constraint terms.</param>
        /// <param name="vars">The variables used in the objective and in the constraints.</param>
        /// <param name="startPoint">The initial guess for the optimal value.</param>
        /// <returns>A sequence of approximate minimizers of <paramref name="objective"/> subject to <paramref name="constraints"/>. The
        /// sequence terminates when the optimizer service determined that it converged.</returns>
        IEnumerable<double[]> Minimize(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint);
    }
}
