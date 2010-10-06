using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    /// Encapsulates a value with an integer index.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class IndexedItem<T>
    {
        /// <summary>
        /// The index of this value in a collection.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// The value from a collection.
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedItem&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public IndexedItem(int index, T value)
        {
            Index = index;
            Value = value;
        }
    }

    /// <summary>
    /// Static methods for creating indexed items.
    /// </summary>
    public static class IndexedItem
    {
        /// <summary>
        /// Creates an indexed item. Exists for convenience because of automatic type inference in C# generics.
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <returns>The created indexed item.</returns>
        public static IndexedItem<T> Create<T>(int index, T value)
        {
            return new IndexedItem<T>(index, value);
        }
    }
}
