using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    class GradientDescentOptimizer : GradientDescentOptimizerBase
    {
        private readonly Func<double[], double[], Term, Variable[], double> stepSizeFunction;

        public GradientDescentOptimizer(Term targetFunction, Variable[] variables, Func<double[], double[], Term, Variable[], double> stepSizeFunction)
            : base(targetFunction, variables)
        {
            Contract.Requires(stepSizeFunction != null);

            this.stepSizeFunction = stepSizeFunction;
        }

        protected override double CalculateStepSize(double[] x, double[] gradient)
        {
            return stepSizeFunction(x, gradient, targetFunction, variables);
        }
    }
}
