using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SysEnumerable = System.Linq.Enumerable;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class Enumerable
    {
        /// <summary>
        /// Flattens an enumeration of enumerations to a single long enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the inner enumeration's items</typeparam>
        /// <typeparam name="S">The type of the inner enumeration</typeparam>
        /// <param name="enumerable">The enumeration to flatten</param>
        /// <returns>The flattened enumeration</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            Contract.Requires(enumerable != null);

            return enumerable.SelectMany(x => x);
        }

        /// <summary>
        /// Generates a possibly infinite sequence of value types using induction.
        /// </summary>
        /// <typeparam name="T">The type of the items in the enumeration.</typeparam>
        /// <param name="init">The initial value</param>
        /// <param name="generator">The induction step that produces the next item using the current item.</param>
        /// <returns>The generated sequence of items.</returns>
        /// <remarks>
        /// If the generator returns <c>null</c> the generation stops and the sequence is finite. If the generator never returns <c>null</c>
        /// the generated sequence is infinite.
        /// </remarks>
        public static IEnumerable<T> Generate<T>(T init, Func<T, T?> generator)
            where T : struct
        {
            T? current = init;
            while (current.HasValue)
            {
                yield return current.Value;
                current = generator(current.Value);
            }
        }
        
        /// <summary>
        /// Creates an enumeration with a single item.
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="item">The item to wrap in an enumeration.</param>
        /// <returns>The enumeration containing only <paramref name="item"/>.</returns>
        public static IEnumerable<T> Singleton<T>(T item)
        {
            return System.Linq.Enumerable.Repeat(item, 1);
        }

        /// <summary>
        /// Appends a single item to the end of an enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the items</typeparam>
        /// <param name="items">The existing enumeration.</param>
        /// <param name="newItem">The new item to append.</param>
        /// <returns>The enumeration resulting from appending <paramref name="newItem"/> to the end of <paramref name="items"/>.</returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, T newItem)
        {
            return items.Concat(Singleton(newItem));
        }

        /// <summary>
        /// Splits an enumeration to multiple enumerations along elements for which a seperator function is <c>false</c>.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source enumeration</typeparam>
        /// <param name="items">The source enumeration</param>
        /// <param name="seperator">The seperator function.</param>
        /// <returns>An enumeration of the resulting splitted enumeration.</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> items, Func<T, bool> seperator)
        {
            while (!items.IsEmpty())
            {
                var head = items.TakeWhile(seperator);
                yield return head;

                var tail = items.SkipWhile(seperator);
                if (!tail.IsEmpty())
                    tail = tail.Skip(1);

                items = tail;
            }
        }

        /// <summary>
        /// Checks if the given enumeration is empty
        /// </summary>
        /// <typeparam name="T">Type of elements in the enumeration</typeparam>
        /// <param name="toCheck">The enumeration to check</param>
        /// <returns><c>true</c> if and only if the enumeration <paramref name="toCheck"/> is empty.</returns>
        [Pure]
        public static bool IsEmpty<T>(this IEnumerable<T> toCheck)
        {
            bool isEmpty = true;
            foreach (var item in toCheck)
            {
                isEmpty = false;
                break;
            }

            return isEmpty;
        }

        /// <summary>
        /// Concats a collection of enumerables with a seperator between them.
        /// </summary>
        /// <typeparam name="T">The type of the items in each enumeration / the type of the seperator</typeparam>
        /// <typeparam name="S">The type of the enumerations</typeparam>
        /// <param name="toConcat">The enumeration of enumerations to concat</param>
        /// <param name="seperator">The seperator to insert</param>
        /// <returns>An enumeration containing all the enumerations in <paramref name="toConcat"/>, with the value <paramref name="seperator"/> as a seperator between them.</returns>
        public static IEnumerable<T> ConcatWithSeperator<T, S>(this IEnumerable<S> toConcat, T seperator)
            where S : IEnumerable<T>
        {
            Contract.Requires(toConcat != null);

            bool first = true;
            foreach (var enumeration in toConcat)
            {
                if (!first)
                    yield return seperator;
                else
                    first = false;

                foreach (var item in enumeration)
                    yield return item;
            }
        }

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
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<IndexedItem<T>>>().Count() == source.Count());

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
            Contract.Requires(source != null);
            Contract.Requires(source.Count() > 1);

            Contract.Ensures(Contract.Result<IEnumerable<Tuple<T, T>>>().Count() == source.Count() - 1);

            return source.Zip(source.Tail());
        }

        /// <summary>
        /// Creates an enumeration of tripples from subsequent elements of a given enumeration.
        /// </summary>
        /// <typeparam name="T">Type of elements in the source enumeration.</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <returns>The resulting sequential tripples.</returns>
        public static IEnumerable<Tuple<T, T, T>> SeqTripples<T>(this IEnumerable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Requires(source.Count() > 2);

            Contract.Ensures(Contract.Result<IEnumerable<Tuple<T, T, T>>>().Count() == source.Count() - 2);

            var pairs = source.SeqPairs();
            var tripples = SysEnumerable.Zip(pairs, source.Skip(2), (pair, item) => Tuple.Create(pair.Item1, pair.Item2, item));
            return tripples;
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
        public static T Minimizer<T, S>(this IEnumerable<T> source, Converter<T, S> itemValue)
            where S: IComparable<S>
        {
            Contract.Requires(source != null);
            Contract.Requires(itemValue != null);
            Contract.Requires(source.IsEmpty() == false);

            Contract.Ensures(source.Contains(Contract.Result<T>())); // the source collection contains the minimizer
            Contract.Ensures(Contract.ForAll(source, item => 
                Comparer<S>.Default.Compare(itemValue(item), itemValue(Contract.Result<T>())) >= 0)); // all items are greater or equal to the minimizer

            var itemsWithKeys = source.Select(x => new { Item = x, Key = itemValue(x) }); 
            var minKey = itemsWithKeys.Min(x => x.Key);             // x.Item2 is itemValue(x). minValue will be the minimum

            // now we take the first item we find that has the minimum value.
            var minimizer = itemsWithKeys.First(pair => pair.Key.CompareTo(minKey) == 0).Item;

            return minimizer;
        }
    }
}
