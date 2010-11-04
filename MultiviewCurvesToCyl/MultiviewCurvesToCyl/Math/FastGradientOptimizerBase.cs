using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    /// <summary>
    /// Performs optimization of a differentiable convex function using the fast gradient scheme from the paper 
    /// "A Fast Iterative Shrinkage-Thresholding Algorithm for Linear Inverse Problems"
    /// </summary>
    [ContractClass(typeof(FastGradientOptimizerBaseContract))]
    abstract class FastGradientOptimizerBase
    {
        protected readonly Term targetFunction;
        protected readonly Variable[] variables;

        /// <summary>
        /// Constructs a new instance of the <see cref="FastGradientOptimizer"/> class.
        /// </summary>
        /// <param name="targetFunction">The target function to be optimized.</param>
        /// <param name="variables">The optimization variables.</param>
        protected FastGradientOptimizerBase(Term targetFunction, Variable[] variables)
        {
            Contract.Requires(targetFunction != null);
            Contract.Requires(variables != null);
            Contract.Requires(Contract.ForAll(variables, variable => variable != null));

            this.targetFunction = targetFunction;
            this.variables = (Variable[])variables.Clone();
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

        [Pure]
        protected abstract double CalculateStepSize(double[] x, double[] gradient);

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
        [Pure]
        protected double F(double[] x)
        {
            Contract.Requires(x.Length == variables.Length);

            return Evaluator.Evaluate(targetFunction, variables, x);
        }

        [Pure]
        protected double[] Gradient(double[] x)
        {
            Contract.Requires(x.Length == variables.Length);
            Contract.Ensures(x.Length == Contract.Result<double[]>().Length);

            return Differentiator.Differentiate(targetFunction, variables, x);
        }

        protected static double[] Diff(double[] x, double[] y)
        {
            Contract.Requires(x.Length == y.Length);
            Contract.Ensures(Contract.Result<double[]>().Length == x.Length);

            var result = new double[x.Length];
            for (int i = 0; i < x.Length; ++i)
                result[i] = x[i] - y[i];
            return result;
        }

        protected static double SquareDiff(double[] x, double[] y)
        {
            Contract.Requires(x.Length == y.Length);
            Contract.Ensures(Contract.Result<double>() >= 0);

            double result = 0;
            for (int i = 0; i < x.Length; ++i)
                result += Math.Pow(x[i] - y[i], 2);
            return result;
        }

        protected static double InnerProduct(double[] x, double[] y)
        {
            Contract.Requires(x.Length == y.Length);

            double result = 0;
            for (int i = 0; i < x.Length; ++i)
                result += x[i] * y[i];
            return result;
        }
    }

    [ContractClassFor(typeof(FastGradientOptimizerBase))]
    abstract class FastGradientOptimizerBaseContract : FastGradientOptimizerBase
    {
        public FastGradientOptimizerBaseContract()
            : base(null, null)
        {
        }

        protected override double CalculateStepSize(double[] x, double[] gradient)
        {
            Contract.Requires(x.Length == gradient.Length);
            Contract.Requires(x.Length == variables.Length);
            Contract.Ensures(Contract.Result<double>() > 0);

            return default(double);
        }
    }

}
