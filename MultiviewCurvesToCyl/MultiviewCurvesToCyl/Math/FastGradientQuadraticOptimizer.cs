using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    /// <summary>
    /// Performs optimization of a convex quadratic function f(x) = <x, Ax> + <b, x>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class assumes that f is indeed a quadratic convex function. It is not given any information about A 
    /// or b, however it can compute b efficiently and also given x, compute Ax efficiently given the assumption
    /// that f is indeed convex and quadratic.
    /// </para>
    /// </remarks>
    class FastGradientQuadraticOptimizer : FastGradientOptimizerBase
    {
        public FastGradientQuadraticOptimizer(Term targetFunction, Variable[] variables)
            : base(targetFunction, variables)
        {
        }

        protected override double CalculateStepSize(double[] x, double[] gradient)
        {
            // we use the fact that the original function is quadratic, therefore the line-search
            // function is quadratic in the step size. We will interpolate the step-size function
            // and find the step-size that minimizes it.

            // the vector (x - gradient). This is the input for step size = 1
            var xOne = x.Zip(gradient, (v, g) => v - g).ToArray();

            // the vector (x - 2 * gradient). This is the input for step size = 2
            var xTwo = x.Zip(gradient, (v, g) => v - 2 * g).ToArray();

            var a = F(x);    // evaluate the value for step size = 0
            var b = F(xOne); // evaluate the value for step size = 1
            var c = F(xTwo); // evaluate the value for step size = 2

            // We will calculate the coefficients of g(s) = alpha * s² + beta * s + gamma, where s is the step size.
            // However we do not need gamma for the minimizer.
            var alpha = (a - 2 * b + c) / 2;
            var beta = (-3 * a + 4 * b - c) / 2;

            Contract.Assume(alpha > 0); // g(s) is a strongly convex parabola
            Contract.Assume(beta < 0);  // g(s) has will have a positive minimizer

            var minimizer = -beta / (2 * alpha);
            Contract.Assert(minimizer > 0);

            return minimizer;
        }

    }
}
