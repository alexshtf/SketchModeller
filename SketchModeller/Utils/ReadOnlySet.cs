using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    /// <summary>
    /// A read-only wrapper around an <see cref="ISet&lt;T&gt;"/>
    /// </summary>
    /// <typeparam name="T">Type of the set's element</typeparam>
    public class ReadOnlySet<T> : ISet<T>
    {
        private readonly ISet<T> wrappedSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlySet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="wrappedSet">The wrapped set.</param>
        public ReadOnlySet(ISet<T> wrappedSet)
        {
            Contract.Requires(wrappedSet != null);

            this.wrappedSet = wrappedSet;
        }

        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return wrappedSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return wrappedSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return wrappedSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return wrappedSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return wrappedSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return wrappedSet.SetEquals(other);
        }

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the set contains the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// 	<c>true</c> if contains the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            return wrappedSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            wrappedSet.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return wrappedSet.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return wrappedSet.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
