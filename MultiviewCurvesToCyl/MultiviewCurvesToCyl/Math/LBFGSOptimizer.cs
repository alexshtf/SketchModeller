using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoDiff;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    class LBFGSOptimizer
    {
        private readonly Term targetFunction;
        private readonly Variable[] variables;

        public LBFGSOptimizer(Term targetFunction, Variable[] variables)
        {
            Contract.Requires(targetFunction != null);
            Contract.Requires(variables != null);
            Contract.Requires(Contract.ForAll(variables, variable => variable != null));

            this.targetFunction = targetFunction;
            this.variables = (Variable[])variables.Clone();
        }

        public double Dimension
        {
            get { return variables.Length; }
        }

        public double[] Minimize(double[] initial)
        {
            Contract.Requires(initial != null);
            Contract.Requires(initial.Length == Dimension);

            alglib.minlbfgsstate state;
            alglib.minlbfgscreate(7, initial, out state);
            alglib.minlbfgssetcond(state, 0, 1E-2, 0, 0);
            alglib.minlbfgsoptimize(state, grad, null, null);

            double[] result;
            alglib.minlbfgsreport report;
            alglib.minlbfgsresults(state, out result, out report);

            return result;
        }

        private void grad(double[] arg, ref double func, double[] grad, object obj)
        {
            func = Evaluator.Evaluate(targetFunction, variables, arg);
            var localGrad = Differentiator.Differentiate(targetFunction, variables, arg);
            Contract.Assume(grad.Length == localGrad.Length);
            Array.Copy(localGrad, grad, grad.Length);
        }
    }
}
