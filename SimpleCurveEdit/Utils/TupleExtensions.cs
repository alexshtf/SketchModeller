using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class TupleExtensions
    {
        public static IEnumerable<T> Enumerate<T>(this Tuple<T> tuple)
        {
            yield return tuple.Item1;
        }

        public static IEnumerable<T> Enumerate<T>(this Tuple<T, T> tuple)
        {
            yield return tuple.Item1;
            yield return tuple.Item2;
        }
    }
}
