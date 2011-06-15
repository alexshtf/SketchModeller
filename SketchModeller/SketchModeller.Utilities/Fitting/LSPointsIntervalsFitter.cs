using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities.Fitting
{
    public class LSPointsIntervalsFitter
    {
        /// <summary>
        /// Computes a least-squares fit for X and Y coordinates such that the intervals are broken at the same indices for both X and Y.
        /// </summary>
        /// <param name="xs">The X coordinates array</param>
        /// <param name="ys">The Y coordinates array</param>
        /// <param name="threshold">Error threahold. For a value of <c>double.NaN</c> the threshold will be automatically selected</param>
        /// <returns>A tuple such that the first and second items are interval coefficients for X and Y respectively. The third item
        /// contains the breaking indices.</returns>
        public static Tuple<double[][], double[][], int[]> FitOptimalIntervals(double[] xs, double[] ys, double threshold = double.NaN)
        {
            Contract.Requires(xs.Length > 0);
            Contract.Requires(xs.Length == ys.Length);
            Contract.Requires(threshold > 0 || double.IsNaN(threshold));

            if (double.IsNaN(threshold))
            {
                var xStd = Math.Sqrt(xs.Variance());
                var yStr = Math.Sqrt(ys.Variance());
                threshold = Math.Max(xStd, yStr) / 50;
            }

            var result = EnumerateFits(xs, ys, threshold).Last(); // fits sequence improves until the last (best) fit is reached.
            return Tuple.Create(result.xIntervals, result.yIntervals, result.breakIndices);
        }

        /// <summary>
        /// Creates an enumeration of fits for X and Y coordinates such that each fit improves the previous one. The last fit is the best one.
        /// </summary>
        /// <param name="xs">The X coordinates array</param>
        /// <param name="ys">The corresponding Y coordinates array</param>
        /// <param name="threshold">Error threshold that controls division into intervals</param>
        /// <returns>The enumeration of fits. Each fit is made of the coefficients and the interval breaking indices</returns>
        private static IEnumerable<Fit> EnumerateFits(double[] xs, double[] ys, double threshold)
        {
            Contract.Requires(xs.Length > 0);
            Contract.Requires(xs.Length == ys.Length);
            Contract.Requires(threshold > 0);

            int n = xs.Length;
            var breakIndices = new List<int> { 3 };

            while (breakIndices.Last() < n) // while we haven't reached our optimal division yet
            {
                var breakIndicesArray = breakIndices.ToArray();
                var xIntervals = LSIntervalsFitter.FitIntervals(xs, breakIndicesArray);
                var yIntervals = LSIntervalsFitter.FitIntervals(ys, breakIndicesArray);
                yield return new Fit { xIntervals = xIntervals, yIntervals = yIntervals, breakIndices = breakIndicesArray };

                var xLastIntervalMSE = LastIntervalMSE(xIntervals, xs, breakIndices);
                var yLastIntervalMSE = LastIntervalMSE(yIntervals, ys, breakIndices);

                if (xLastIntervalMSE < threshold && yLastIntervalMSE < threshold) // our current interval is still good enough
                    breakIndices[breakIndices.Count - 1] = breakIndices.Last() + 1;
                else // we need a new interval to approximate well enough
                {
                    var nextIntervalBreak = breakIndices.Last() + 3;
                    if (nextIntervalBreak >= n)
                        breakIndices[breakIndices.Count - 1] = breakIndices.Last() + 1;
                    else
                        breakIndices.Add(nextIntervalBreak);
                }
            }
        }

        /// <summary>
        /// Computes the mean squared error of the last interval 
        /// </summary>
        /// <param name="intervals">The list of intervals coefficients</param>
        /// <param name="values">The values used to fit the intervals</param>
        /// <param name="breakIndices">The interval breaking indices in <paramref name="values"/></param>
        /// <returns>The MSE of the last interval</returns>
        private static double LastIntervalMSE(double[][] intervals, double[] values, IList<int> breakIndices)
        {
            var intervalEnd = breakIndices.Last();
            var intervalStart = breakIndices.Count == 1 ? 0 : breakIndices.Take(breakIndices.Count - 1).Last();
            var pointsCount = intervalEnd - intervalStart + 1;

            var lastIntervalCoefficients = intervals.Last();
            var query =
                from t in Enumerable.Range(intervalStart, pointsCount)
                let realValue = values[t]
                let predictedValue = // 3rd degree polynomial
                    lastIntervalCoefficients[0] +
                    lastIntervalCoefficients[1] * t +
                    lastIntervalCoefficients[2] * t * t +
                    lastIntervalCoefficients[3] * t * t * t
                let residual = realValue - predictedValue
                select residual * residual;

            return query.Average();
        }

        private struct Fit
        {
            public double[][] xIntervals;
            public double[][] yIntervals;
            public int[] breakIndices;
        }
    }
}
