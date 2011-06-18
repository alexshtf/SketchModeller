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
        /// Generates an infinite sequence of values given a seed and a generator function that
        /// transforms an element to the next element in the sequence.
        /// </summary>
        /// <typeparam name="T">The type of elements in the sequence</typeparam>
        /// <param name="seed">The first element in the sequence</param>
        /// <param name="generator">A function that transforms an element to the next one</param>
        /// <returns>An infinite sequence of elements [ seed, generator(seed), generator(generator(seed)), ... ].</returns>
        public static IEnumerable<T> Generate<T>(T seed, Func<T, T> generator)
        {
            while (true)
            {
                yield return seed;
                seed = generator(seed);
            }
        }

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
