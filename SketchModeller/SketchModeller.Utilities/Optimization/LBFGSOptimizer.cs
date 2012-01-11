using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace SketchModeller.Utilities.Optimization
{
    public class LBFGSOptimizer : IFirstOrderUnconstrainedOptimizer
    {
        public double[] Solve(Func<double[], Tuple<double[], double>> objectiveWithGradient, double[] initialValue, double gradientNormThreshold)
        {
            alglib.minlbfgsstate state;
            alglib.minlbfgscreate(Math.Min(3, initialValue.Length), initialValue, out state);
            alglib.minlbfgssetcond(state, gradientNormThreshold, 0, 0, 1000);

            alglib.minlbfgsoptimize(state,
                (double[] arg, ref double func, double[] gradient, object obj) =>
                {
                    var gradientAndValue = objectiveWithGradient(arg);
                    func = gradientAndValue.Item2;
                    Array.Copy(gradientAndValue.Item1, gradient, gradient.Length);
                }, null, null);

            alglib.minlbfgsreport report;
            double[] result;
            alglib.minlbfgsresults(state, out result, out report);

            return result;
        }
    }
}
