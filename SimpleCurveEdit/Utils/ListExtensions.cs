using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Utils
{
    public static class ListExtensions
    {
        public static ListSlice<T> Slice<T>(this IList<T> list, int start, int end)
        {
            return new ListSlice<T>(list, start, end);
        }

        public static ReadOnlyCollection<T> AsReadOnly<T>(this IList<T> list)
        {
            return new ReadOnlyCollection<T>(list);
        }
    }
}
