using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class SingletonSet
    {
        public static SingletonSet<T> ToSingletonSet<T>(this T item)
        {
            return new SingletonSet<T>(item);
        }
    }

    public class SingletonSet<T> : ISet<T>
    {
        private readonly T item;

        public SingletonSet(T item)
        {
            this.item = item;
        }

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
            // contains item and has at-least two items
            if (other.Contains(item) && other.Skip(1).Any()) 
                return true;
            else
                return false;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            // a singleton is a proper superset ONLY of the empty set
            if (!other.Any())
                return true;
            else
                return false;
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            // we are a subset if the other enumeration contains our single item
            if (other.Contains(item))
                return true;
            else
                return false;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            // we are a superset of the empty set
            if (!other.Any())
                return true;

            // otherwise, we are a superset of another singleton with the same item.
            return SetEquals(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            // we overlap another set if and only if it contains our item
            return other.Contains(item);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            // the other set equals us if it contains a single item that is equal to our item
            return
                other.Any() &&              // other is not empty
                !other.Skip(1).Any() &&     // other contains at most one item
                item.Equals(other.First()); // other's item is equal to our item.
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
            if (this.item.Equals(item))
                return true;
            else
                return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array[arrayIndex] = item;
        }

        public int Count
        {
            get { return 1; }
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
            yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
