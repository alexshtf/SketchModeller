using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Creates delegate comparers in a convenient way
    /// </summary>
    public static class DelegateComparer
    {
        /// <summary>
        /// Creates a delegate comparer given a comparison delegate
        /// </summary>
        /// <typeparam name="T">The type of the compared objects</typeparam>
        /// <param name="comparison">The comparison.</param>
        /// <returns>The resulting delegate comparer</returns>
        public static DelegateComparer<T> Create<T>(Comparison<T> comparison)
        {
            return new DelegateComparer<T>(comparison);
        }
    }

    /// <summary>
    /// Wraps a <c>Comparison&lt;T&gt;</c> delegate inside an implementation of <c>IComparer&lt;T&gt;</c> interface
    /// </summary>
    /// <typeparam name="T">The compared type</typeparam>
    public class DelegateComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> comparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparison">The comparison delegate</param>
        public DelegateComparer(Comparison<T> comparison)
        {
            Contract.Requires(comparison != null);

            this.comparison = comparison;
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than,
        /// equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of x and y. Less than zero - x is less than y. Zero - x
        /// equals y. Greater than zero - x is greater than y.
        /// </returns>
        public int Compare(T x, T y)
        {
            return comparison(x, y);
        }
    }
}
