using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    /// <summary>
    /// Various utilities for .NET collections
    /// </summary>
    public static class CollectionUtils
    {
        /// <summary>
        /// Adds multiple items to a collection.
        /// </summary>
        /// <typeparam name="T">Type of the items</typeparam>
        /// <param name="target">The target collection.</param>
        /// <param name="newItems">The new items.</param>
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
                target.Add(item);
        }

        /// <summary>
        /// Adds multiple items to a collection. Similar to <see cref="AddRange"/> but has <c>params</c> syntax.
        /// </summary>
        /// <typeparam name="T">Type of the items</typeparam>
        /// <param name="target">The target collection.</param>
        /// <param name="items">The new items.</param>
        public static void AddMany<T>(this ICollection<T> target, params T[] items)
        {
            target.AddRange(items);
        }

        /// <summary>
        /// Returns a read-only wrapper around the specified set.
        /// </summary>
        /// <typeparam name="T">The type of set elements.</typeparam>
        /// <param name="set">The set.</param>
        /// <returns>A read-only wrapper around the specified set.</returns>
        public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set)
        {
            Contract.Requires(set != null);

            return new ReadOnlySet<T>(set);
        }
    }
}
