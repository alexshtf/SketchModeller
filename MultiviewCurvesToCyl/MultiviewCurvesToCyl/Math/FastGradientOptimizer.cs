using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace MultiviewCurvesToCyl
{
    class FastGradientOptimizer : FastGradientOptimizerBase
    {
        private readonly Func<double[], double[], Term, Variable[], double> stepSizeFunction;

        public FastGradientOptimizer(
            Term targetFunction,
            Variable[] variables,
            [Pure] Func<double[], double[], Term, Variable[], double> stepSizeFunction)
            : base(targetFunction, variables)
        {
            Contract.Requires(stepSizeFunction != null);

            this.stepSizeFunction = stepSizeFunction;
        }

        public static double InexactStepSize(double[] x, double[] gradient, Term targetFunction, Variable[] variables, double max, double factor, double min)
        {
            // arrays are ok
            Contract.Requires(x != null);
            Contract.Requires(gradient != null);
            Contract.Requires(x.Length == gradient.Length);
            Contract.Requires(x.Length == variables.Length);

            // target + variables are ok.
            Contract.Requires(targetFunction != null);
            Contract.Requires(variables != null);
            Contract.Requires(Contract.ForAll(variables, variable => variable != null));

            // min/max/factor are ok.
            Contract.Requires(max > min);
            Contract.Requires(factor > 1);
            Contract.Requires(min > 0);

            var valueAtX = Evaluator.Evaluate(targetFunction, variables, x);

            double bestStepSize = max;
            double minValue = double.MaxValue;
            double currStepSize = max;
            // the loop will happen until: 
            //  - the current step size races min 
            //      AND 
            //  - we actually decrease the objective function value
            while (currStepSize >= min || minValue > valueAtX)
            {
                var currInput = new double[x.Length];
                for (int i = 0; i < currInput.Length; ++i)
                    currInput[i] = x[i] - currStepSize * gradient[i];
                var currValue = Evaluator.Evaluate(targetFunction, variables, currInput);
                if (currValue < minValue)
                {
                    minValue = currValue;
                    bestStepSize = currStepSize;
                }
                currStepSize = currStepSize / factor;
            }

            return bestStepSize;
        }

        protected override double CalculateStepSize(double[] x, double[] gradient)
        {
            return stepSizeFunction(x, gradient, targetFunction, variables);
        }

        private static IEnumerable<double> GenerateStepSize(double min, double max, double factor)
        {
            double current = max;
            while (current >= min)
            {
                yield return current;
                current = current / factor;
            }
        }
    }
}
