using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public static class Histogram
    {
        public static double[] Probabilities(IEnumerable<double> values, int nbins)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Any());
            Contract.Requires(nbins >= 0);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == nbins);
            Contract.Ensures(Contract.ForAll(Contract.Result<double[]>(), p => p >= 0 && p <= 1));

            var counts = Counts(values, nbins);
            var max = counts.Max();
            var result = counts.Select(c => c / (double)max).ToArray();

            return result;
        }

        /// <summary>
        /// Computes a uniform counts histogram for a sequence of values
        /// </summary>
        /// <param name="values">The values</param>
        /// <param name="nbins">The number of histogram bins</param>
        /// <returns>An array A such that <c>A[i]</c> is the number of elements in bin <c>i</c>.</returns>
        public static int[] Counts(IEnumerable<double> values, int nbins)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Any());
            Contract.Requires(nbins >= 0);
            Contract.Ensures(Contract.Result<int[]>() != null);
            Contract.Ensures(Contract.Result<int[]>().Length == nbins);
            Contract.Ensures(Contract.ForAll(Contract.Result<int[]>(), count => count >= 0));

            var max = values.Max();
            var min = values.Min();
            var binSize = (max - min) / nbins;
            var result = new int[nbins];
            foreach (var value in values)
            {
                int bin = (int)Math.Floor((value - min) / binSize);
                result[bin]++;
            }

            return result;
        }
    }
}
