using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SysEnumerable = System.Linq.Enumerable;

namespace Utils
{
    public static class Enumerable
    {
        /// <summary>
        /// Creates an enumeration with all but the first element in a source enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the enumeration</typeparam>
        /// <param name="source">The enumeration to take the tail of.</param>
        /// <returns>The resulting enumeration.</returns>
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> source)
        {
            return source.Skip(1);
        }

        /// <summary>
        /// Zips two enumerations creating an enumeration of pairs.
        /// </summary>
        /// <typeparam name="T">Type of elements in he first enumeration</typeparam>
        /// <typeparam name="S">Tupe of elements in the second enumeration</typeparam>
        /// <param name="first">First enumeration</param>
        /// <param name="second">Second enumeration</param>
        /// <returns>Pairs of zipped elements</returns>
        public static IEnumerable<Tuple<T, S>> Zip<T, S>(this IEnumerable<T> first, IEnumerable<S> second)
        {
            return first.Zip(second, (x, y) => Tuple.Create(x, y));
        }

        /// <summary>
        /// Zips each element with its index in the source enumeration.
        /// </summary>
        /// <typeparam name="T">Type of elements in the source enumeration</typeparam>
        /// <param name="source">The source enumeration</param>
        /// <returns>An enumeration of structueres containing items from <paramref name="source"/> with their index</returns>
        public static IEnumerable<IndexedItem<T>> ZipIndex<T>(this IEnumerable<T> source)
        {
            return source.ZipIndex(0);
        }

        /// <summary>
        /// Zips each element with its index in the source enumeration.
        /// </summary>
        /// <typeparam name="T">Type of elements in the source enumeration</typeparam>
        /// <param name="source">The source enumeration</param>
        /// <param name="offset">An offset value to add to all the indices</param>
        /// <returns>An enumeration of structueres containing items from source with their index + <paramref name="offset"/></returns>
        public static IEnumerable<IndexedItem<T>> ZipIndex<T>(this IEnumerable<T> source, int offset)
        {
            var count = source.Count();
            return 
                from pair in source.Zip(SysEnumerable.Range(offset, count))
                let index = pair.Item2
                let value = pair.Item1
                select IndexedItem.Create(index, value);
        }

        /// <summary>
        /// Creates an enumeration of sequential pairs from the source enumeration. That is, if the enumeration <paramref name="source"/>
        /// contains {x1, x2, x3, x4, ....} then the result will contain the pairs {(x1, x2), (x2, x3), (x3, x4), ... }
        /// </summary>
        /// <typeparam name="T">Type of elements in the source enumeration</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <returns>The resulting sequential pairs.</returns>
        public static IEnumerable<Tuple<T, T>> SeqPairs<T>(this IEnumerable<T> source)
        {
            return source.Zip(source.Tail());
        }

        /// <summary>
        /// Returns an item from the source enumeration that minimizes a certain value.
        /// </summary>
        /// <typeparam name="T">The type of items in the source enumeration</typeparam>
        /// <typeparam name="S">The item of values to minimize</typeparam>
        /// <param name="source">The enumeration of items to minimize over</param>
        /// <param name="itemValue">The function to calculate item value for each item.</param>
        /// <returns><c>x</c> from <paramref name="source"/> that minimizes itemValue(x)</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source"/> is an empty enumeration.</exception>
        public static T Minimizer<T, S>(this IEnumerable<T> source, Func<T, S> itemValue)
            where S: IComparable<S>
        {
            if (source.GetEnumerator().MoveNext() == false) // we got an empty enumeration
                throw new ArgumentException("Cannot get the minimizer of an empty enumeration");

            var pairs = source.Zip(source.Select(itemValue));   // zip items with their values
            var minValue = pairs.Min(x => x.Item2);             // x.Item2 is itemValue(x). minValue will be the minimum

            // now we take the first item we find that has the minimum value.
            var minimizer = pairs.First(pair => pair.Item2.CompareTo(minValue) == 0).Item1;

            return minimizer;
        }
    }
}
