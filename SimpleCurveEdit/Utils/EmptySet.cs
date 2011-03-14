using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public class EmptySet<T> : ISet<T>
    {
        public static readonly EmptySet<T> Instance = new EmptySet<T>();

        public bool Add(T item)
        {
            throw new NotSupportedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other.Any())
                return true;
            else
                return false;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return false;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (!other.Any())
                return true;
            else
                return false;
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (!other.Any())
                return true;
            else
                return false;
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            // Do nothing. This is the empty set.
        }

        public int Count
        {
            get { return 0; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
