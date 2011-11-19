using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Utils
{
    /// <summary>
    /// Static method for operating on .NET Lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Creates a list slice. A list slice is a read-only list that exposes a sub-list of another list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to slice from</param>
        /// <param name="start">The inclusive index of the first item in <paramref name="list"/>.</param>
        /// <param name="end">The exclusive index of the last item in <paramref name="list"/>.</param>
        /// <returns>A list object that exposes the items between <paramref name="start"/> and <paramref name="end"/> in
        /// the list <paramref name="list"/>.</returns>
        public static ListSlice<T> Slice<T>(this IList<T> list, int start, int end)
        {
            return new ListSlice<T>(list, start, end);
        }

        /// <summary>
        /// Creates a read only collection wrapping the specified list.
        /// </summary>
        /// <typeparam name="T">Type of list items</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>A read only collection wrapping <paramref name="list"/>.</returns>
        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list)
        {
            return new ReadOnlyCollection<T>(list);
        }
    }
}
