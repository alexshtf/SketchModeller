using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Computes the variance of the specified sequence of numbers.
        /// </summary>
        /// <param name="values">The sequence of numbers.</param>
        /// <returns>The variance of the values in <paramref name="values"/>.</returns>
        public static double Variance(this IEnumerable<double> values)
        {
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<double>() >= 0);

            var squared = values.Select(x => x * x);
            var avg = values.Average();

            // var(X) = E(X²) - E²(X)
            return squared.Average() - avg * avg;
        }
    }
}
