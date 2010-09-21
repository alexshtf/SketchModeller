using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class CollectionUtils
    {
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> newItems)
        {
            foreach (var item in newItems)
                target.Add(item);
        }

        public static void AddMany<T>(this ICollection<T> target, params T[] items)
        {
            target.AddRange(items);
        }
    }
}
