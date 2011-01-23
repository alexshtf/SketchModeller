using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace SketchModeller.Modelling.Services.Snap
{
    class Optimizer
    {
        public static double[] Minimize(Term targetFunc, Variable[] vars)
        {
            double[] x = new double[vars.Length];
            alglib.minlbfgsstate state;
            alglib.minlbfgscreate(1, x, out state);

            var gradProvider = new GradProvider(targetFunc, vars);
            alglib.minlbfgsoptimize(state, gradProvider.Grad, null, null);

            alglib.minlbfgsreport rep;
            alglib.minlbfgsresults(state, out x, out rep);

            Trace.WriteLine("Termination type is " + TranslateTerminationType(rep.terminationtype));
            Trace.WriteLine("Number of function evals is " + rep.nfev);
            Trace.WriteLine("Iterations count is " + rep.iterationscount);

            if (rep.terminationtype > 0) // good. no errors
                return x;
            else
                throw new InvalidOperationException("Iteration did not converge!");
        }

        private static string TranslateTerminationType(int terminationtype)
        {
            switch (terminationtype)
            {
                case -2:
                    return "rounding errors prevent further improvement.";
                case -1:
                    return "incorrect parameters were specified";
                case 1:
                    return "relative function improvement is no more than EpsF";
                case 2:
                    return "relative step is no more than EpsX";
                case 4:
                    return "gradient norm is no more than EpsG";
                case 5:
                    return "MaxIts steps was taken";
                case 7:
                    return "stopping conditions are too stringent, further improvement is impossible";
                default:
                    return "unknown";
            }
        }

        private class GradProvider
        {
            private readonly Term targetFunc;
            private readonly Variable[] vars;

            public GradProvider(Term targetFunc, Variable[] vars)
            {
                this.targetFunc = targetFunc;
                this.vars = vars;
            }

            public void Grad(double[] arg, ref double func, double[] grad, object obj)
            {
                func = Evaluator.Evaluate(targetFunc, vars, arg);
                var tempGrad = Differentiator.Differentiate(targetFunc, vars, arg);
                Contract.Assume(tempGrad.Length == grad.Length);
                Array.Copy(tempGrad, grad, grad.Length);
            }
        }
    }
}
