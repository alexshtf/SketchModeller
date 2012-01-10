using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// Generates an infinite sequence of augmented lagrangian algorithm iterations.
    /// </summary>
    public interface IAugmentedLagrangianIterations
    {
        /// <summary>
        /// Starts the iterations sequence of the optimization algorithm.
        /// </summary>
        /// <param name="objective">The objective function term.</param>
        /// <param name="constraints">The constraint terms</param>
        /// <param name="variables">The variables used in the objective and the constraints.</param>
        /// <param name="startPoint">The initial guess for the optimal value.</param>
        /// <returns>An infinite sequence of iteration results. The user of this sequence decides when to break the iteration
        /// according to some convergence test.</returns>
        IEnumerable<AugmentedLagrangianIterationResult> Start(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint);
    }
}
