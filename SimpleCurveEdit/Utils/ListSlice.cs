using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    /// A read only view of a list between two indices.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListSlice<T> : IList<T>
    {
        private readonly IList<T> list;
        private readonly int start;
        private readonly int end;

        public ListSlice(IList<T> list, int start, int end)
        {
            this.list = list;
            this.start = start;
            this.end = end;
        }

        public int IndexOf(T item)
        {
            var indexOf = list.IndexOf(item);
            if (indexOf >= start && indexOf < end)
                return indexOf - start;
            else
                return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Insertion not supported for slices");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("Removal not supported for slices");
        }

        public T this[int index]
        {
            get
            {
                if (index >= end - start)
                    throw new ArgumentOutOfRangeException("index is beyond the size of this slice");
                else
                    return list[start + index];
            }
            set
            {
                if (index >= end - start)
                    throw new ArgumentOutOfRangeException("index is boyond the size of this slice");
                else
                    list[start + index] = value;
            }
        }

        public void Add(T item)
        {
            throw new NotSupportedException("Adding is not supported to slices");
        }

        public void Clear()
        {
            throw new NotSupportedException("Clearing is not supported for slices");
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int j = arrayIndex;
            for (int i = start; i < end; ++i, j++)
                array[j] = list[i];
        }

        public int Count
        {
            get { return end - start; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("Removal is not supported for slices");
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = start; i < end; ++i)
                yield return list[i];
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
