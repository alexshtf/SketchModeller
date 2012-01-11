using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    public class ConjugateGradientOptimizer : IFirstOrderUnconstrainedOptimizer
    {
        public double[] Solve(Func<double[], Tuple<double[], double>> objectiveWithGradient, double[] initialValue, double gradientNormThreshold)
        {
            alglib.mincgstate state;
            alglib.mincgcreate(initialValue, out state);
            alglib.mincgsetcond(state, gradientNormThreshold, 0, 0, 1000);
            alglib.mincgoptimize(state, 
                (double[] arg, ref double func, double[] gradient, object obj) =>
                {
                    var gradientAndValue = objectiveWithGradient(arg);
                    func = gradientAndValue.Item2;
                    Array.Copy(gradientAndValue.Item1, gradient, gradient.Length);
                }, null, null);

            alglib.mincgreport report;
            double[] result;
            alglib.mincgresults(state, out result, out report);
            return result;
        }
    }
}
