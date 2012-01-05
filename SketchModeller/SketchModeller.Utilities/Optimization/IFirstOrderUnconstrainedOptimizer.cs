using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// An interface to a first-order unconstrained optimizer (requires only the gradient)
    /// </summary>
    public interface IFirstOrderUnconstrainedOptimizer
    {
        /// <summary>
        /// Solves an optimization problem given the objective function and an initial value.
        /// </summary>
        /// <param name="objectiveWithGradient">A function that computes the value and the gradient of the objective</param>
        /// <param name="initialValue">The initial guess for the optimal solution</param>
        /// <returns>The computed optimal solution</returns>
        double[] Solve(Func<double[], Tuple<double[], double>> objectiveWithGradient, double[] initialValue);
    }
}
