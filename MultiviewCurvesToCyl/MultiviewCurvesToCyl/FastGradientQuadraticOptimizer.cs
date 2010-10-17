using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    class OptimizationStepResult
    {
        public double CurrentTarget { get; set; }
        public double PrevTarget { get; set; }
        public double[] CurrentMinimizer { get; set; }
        public double[] PrevMinimizer { get; set; }
    }

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
    class FastGradientQuadraticOptimizer
    {
        /*
         * This class computes the minimum using the fast gradient scheme described in the paper 
         * "A Fast Iterative Shrinkage-Thresholding Algorithm for Linear Inverse Problems" however we
         * do not minimize a non-differentiable function and therefore the implementation here is easier
         */

        private readonly Term targetFunction;
        private readonly Variable[] variables;

        public FastGradientQuadraticOptimizer(Term targetFunction, Variable[] variables)
        {
            this.targetFunction = targetFunction;
            this.variables = variables;
        }

        /// <summary>
        /// Returns an infinite enumeration of optimization step results. It's up to the user to 
        /// stop the iteration.
        /// </summary>
        /// <param name="initial">The initial guess for the minimizer to start with</param>
        /// <returns></returns>
        public IEnumerable<OptimizationStepResult> Minimize(double[] initial)
        {
            var x = (double[])initial.Clone();
            var value = F(x);

            var t = 1.0;
            var y = (double[])x.Clone();

            while (true)
            {
                // find optimal step size
                var yGradient = Gradient(y);
                var stepSize = CalculateStepSize(y, yGradient);
                var l = 1 / stepSize;
                
                // find new minimizer approximation
                var xPrev = x;
                var valuePrev = value;
                x = PL(l, y, yGradient);
                value = F(x);
                
                // return a value to the user
                yield return new OptimizationStepResult
                {
                    CurrentMinimizer = x,
                    PrevMinimizer = xPrev,
                    CurrentTarget = value,
                    PrevTarget = valuePrev,
                };

                // calculate new t
                var tPrev = t;
                t = (1 + Math.Sqrt(1 + 4 * t * t)) / 2;

                // calculate new y
                y = new double[y.Length];
                var factor = tPrev / t;
                for (int i = 0; i < y.Length; ++i)
                    y[i] = x[i] + factor * (x[i] - xPrev[i]);
            }
        }

        /// <summary>
        /// Computes the P_L(y), as specified in the paper
        /// </summary>
        /// <param name="l">The value of L</param>
        /// <param name="y">The vector y</param>
        /// <returns>The vector P_L(x) as specified in the paper</returns>
        private double[] PL(double l, double[] y, double[] yGradient)
        {
            Contract.Requires(y.Length == variables.Length);
            Contract.Requires(yGradient.Length == y.Length);
            Contract.Requires(l > 0);
            Contract.Ensures(Contract.Result<double[]>().Length == variables.Length);

            var factor = 1 / l;
            var result = new double[y.Length];
            for (int i = 0; i < y.Length; ++i)
                result[i] = y[i] - factor * yGradient[i];
            return result;
        }

        /// <summary>
        /// Computes the value of the target function
        /// </summary>
        /// <param name="x">The input</param>
        /// <returns>The target function's value for a vector <c>x</c>.</returns>
        private double F(double[] x)
        {
            Contract.Requires(x.Length == variables.Length);

            return Evaluator.Evaluate(targetFunction, variables, x);
        }

        private double[] Gradient(double[] x)
        {
            Contract.Requires(x.Length == variables.Length);
            Contract.Ensures(x.Length == Contract.Result<double[]>().Length);

            return Differentiator.Differentiate(targetFunction, variables, x);
        }

        private static double[] Diff(double[] x, double[] y)
        {
            Contract.Requires(x.Length == y.Length);
            Contract.Ensures(Contract.Result<double[]>().Length == x.Length);

            var result = new double[x.Length];
            for (int i = 0; i < x.Length; ++i)
                result[i] = x[i] - y[i];
            return result;
        }

        private static double SquareDiff(double[] x, double[] y)
        {
            Contract.Requires(x.Length == y.Length);
            Contract.Ensures(Contract.Result<double>() >= 0);

            double result = 0;
            for (int i = 0; i < x.Length; ++i)
                result += Math.Pow(x[i] - y[i], 2);
            return result;
        }

        private static double InnerProduct(double[] x, double[] y)
        {
            Contract.Requires(x.Length == y.Length);

            double result = 0;
            for (int i = 0; i < x.Length; ++i)
                result += x[i] * y[i];
            return result;
        }

        [Pure]
        private double CalculateStepSize(double[] x, double[] gradient)
        {
            Contract.Requires(x.Length == gradient.Length);
            Contract.Requires(x.Length == variables.Length);
            Contract.Ensures(Contract.Result<double>() > 0);

            // we use the fact that the original function is quadratic, therefore the line-search
            // function is quadratic in the step size. We will interpolate the step-size function
            // and find the step-size that minimizes it.

            // the vector (x - gradient). This is the input for step size = 1
            var xOne = x.Zip(gradient, (v, g) => v - g).ToArray();

            // the vector (x - 2 * gradient). This is the input for step size = 2
            var xTwo = x.Zip(gradient, (v, g) => v - 2 * g).ToArray();

            var a = Evaluator.Evaluate(targetFunction, variables, x);    // evaluate the value for step size = 0
            var b = Evaluator.Evaluate(targetFunction, variables, xOne); // evaluate the value for step size = 1
            var c = Evaluator.Evaluate(targetFunction, variables, xTwo); // evaluate the value for step size = 2

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
