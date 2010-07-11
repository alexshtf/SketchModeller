using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SysEnumerable = System.Linq.Enumerable;

namespace Utils
{
    public static class Enumerable
    {

        public static IEnumerable<Tuple<T, T>> SeqPairs<T>(this IEnumerable<T> source)
        {
            return source.Zip(source.Skip(1), (x, y) => Tuple.Create(x, y));
        }

        public static IEnumerable<IndexedItem<T>> ZipIndex<T>(this IEnumerable<T> source)
        {
            return source.ZipIndex(0);
        }

        public static IEnumerable<IndexedItem<T>> ZipIndex<T>(this IEnumerable<T> source, int baseIndex)
        {
            var count = source.Count();
            return source.Zip(SysEnumerable.Range(baseIndex, count), (value, index) => IndexedItem.Create(index, value));
        }
    }
}
