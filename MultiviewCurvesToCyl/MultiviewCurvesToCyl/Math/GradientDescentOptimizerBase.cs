using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MultiviewCurvesToCyl
{
    [ContractClass(typeof(GradientDescentOptimizerBaseContract))]
    abstract class GradientDescentOptimizerBase
    {
        protected readonly Term targetFunction;
        protected readonly Variable[] variables;

        public GradientDescentOptimizerBase(Term targetFunction, Variable[] variables)
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

            while (true)
            {
                var grad = Gradient(x);
                var stepSize = CalculateStepSize(x, grad);

                // remember old x, value
                var xPrev = x;
                var valuePrev = value;

                // perform gradient step
                x = new double[xPrev.Length];
                for (int i = 0; i < x.Length; ++i)
                    x[i] = xPrev[i] - stepSize * grad[i];
                value = F(x);

                yield return new OptimizationStepResult
                {
                    CurrentMinimizer = x,
                    PrevMinimizer = xPrev,
                    CurrentTarget = value,
                    PrevTarget = valuePrev,
                };
            }
        }

        protected abstract double CalculateStepSize(double[] x, double[] gradient);

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
    }


    [ContractClassFor(typeof(GradientDescentOptimizerBase))]
    abstract class GradientDescentOptimizerBaseContract : GradientDescentOptimizerBase
    {
        public GradientDescentOptimizerBaseContract()
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
