using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class IndexedItem<T>
    {
        public readonly int Index;
        public readonly T Value;

        public IndexedItem(int index, T value)
        {
            Index = index;
            Value = value;
        }
    }

    public static class IndexedItem
    {
        public static IndexedItem<T> Create<T>(int index, T value)
        {
            return new IndexedItem<T>(index, value);
        }
    }
}
