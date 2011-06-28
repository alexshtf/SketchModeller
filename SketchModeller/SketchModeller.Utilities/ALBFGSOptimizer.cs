using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities
{
    public static class ALBFGSOptimizer
    {
        public static double[] Minimize(
            Term target,
            Term[] constraints,
            Variable[] vars,
            double[] x,
            double mu = 1,
            double tolerance = 1E-6)
        {
            return ALOptimizer.Minimize(
                target, 
                constraints, 
                vars, 
                x, 
                LBFGSMinimizer, 
                mu, 
                tolerance);
        }

        private static double[] LBFGSMinimizer(
          Func<double[], Tuple<double, double[]>> computeGradient,
          double[] x)
        {
            alglib.minlbfgsstate state;
            alglib.minlbfgscreate(3, x, out state);

            alglib.minlbfgsoptimize(state,
                (double[] arg, ref double func, double[] gradient, object obj) =>
                {
                    var gradientAndValue = computeGradient(arg);
                    func = gradientAndValue.Item1;
                    Array.Copy(gradientAndValue.Item2, gradient, gradient.Length);
                }, null, null);

            alglib.minlbfgsreport report;
            double[] result;
            alglib.minlbfgsresults(state, out result, out report);

            return result;
        }
    }
}
