using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// Serves as a convergence test for augmented lagrangian implementations.
    /// </summary>
    public interface IConvergenceTest
    {
        /// <summary>
        /// Resets the convergence testing class to the state as if it was just created.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Updates the convergence testing class with the current state of iteration results.
        /// </summary>
        /// <param name="iterationResult">The current iteration result.</param>
        void Update(AugmentedLagrangianIterationResult iterationResult);

        /// <summary>
        /// Returns the result of the convergence test - <c>true</c> if and only if the algorithm has converged.
        /// </summary>
        bool HasConverged { get; }
    }
}
