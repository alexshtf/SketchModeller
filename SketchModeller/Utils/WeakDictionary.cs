using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private Dictionary<object, SafeWeakRef<TValue>> dictionary;
        private WeakKeyComparer<TKey> comparer;

        public WeakDictionary()
            : this(0, null) { }

        public WeakDictionary(int capacity)
            : this(capacity, null) { }

        public WeakDictionary(IEqualityComparer<TKey> comparer)
            : this(0, comparer) { }

        public WeakDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            this.comparer = new WeakKeyComparer<TKey>(comparer);
            this.dictionary = new Dictionary<object, SafeWeakRef<TValue>>(capacity, this.comparer);
        }

        public void Add(TKey key, TValue value)
        {
            var weakKey = new WeakKeyRef<TKey>(key, this.comparer);
            var weakValue = new SafeWeakRef<TValue>(value);
            dictionary.Add(weakKey, weakValue);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { throw new NotSupportedException(); }
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            SafeWeakRef<TValue> weakValue;
            if (dictionary.TryGetValue(key, out weakValue))
            {
                value = weakValue.Target;
                return weakValue.IsAlive;
            }
            value = null;
            return false;
        }

        public ICollection<TValue> Values
        {
            get
            {
                var result = new List<TValue>();
                foreach (var safeWeakRef in dictionary.Values)
                {
                    var target = safeWeakRef.Target;
                    if (target != null)
                        result.Add(target);
                }

                return result;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue result;
                if (TryGetValue(key, out result))
                    return result;
                else
                    throw new KeyNotFoundException();
            }
            set
            {
                var weakKey = new WeakKeyRef<TKey>(key, comparer);
                this.dictionary[weakKey] = new SafeWeakRef<TValue>(value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(item.Value, this[item.Key]);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
                return Remove(item.Key);
            else
                return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var kvp in dictionary)
            {
                var weakKey = (SafeWeakRef<TKey>)(kvp.Key);
                var weakValue = kvp.Value;

                TKey key = weakKey.Target;
                TValue value = weakValue.Target;

                if (weakKey.IsAlive && weakValue.IsAlive)
                    yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void RemoveCollectedEntries()
        {
            List<object> toRemove = null;
            foreach (var pair in this.dictionary)
            {
                var weakKey = (SafeWeakRef<TKey>)(pair.Key);
                var weakValue = pair.Value;

                if (!weakKey.IsAlive || !weakValue.IsAlive)
                {
                    if (toRemove == null)
                        toRemove = new List<object>();
                    toRemove.Add(weakKey);
                }
            }

            if (toRemove != null)
            {
                foreach (object key in toRemove)
                    this.dictionary.Remove(key);
            }
        }


        #region helper classes

        /// <summary>
        /// A weak reference object with a type-safe target
        /// </summary>
        /// <typeparam name="T">Type of the target</typeparam>
        private class SafeWeakRef<T> : WeakReference
            where T : class
        {
            public SafeWeakRef(T obj)
                : base(obj)
            {
            }

            public new T Target
            {
                get { return (T)base.Target; }
            }
        }

        /// <summary>
        /// A type-safe weak reference object that points to <c>null</c>. It is always considered alive.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        private class WeakNullRef<T> : SafeWeakRef<T>
            where T : class
        {
            public WeakNullRef()
                : base(null)
            {
            }

            public override bool IsAlive
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class WeakKeyRef<T> : SafeWeakRef<T> where T : class
        {
            public readonly int HashCode;

            public WeakKeyRef(T key, WeakKeyComparer<T> comparer)
                : base(key)
            {
                // retain the object's hash code immediately so that even
                // if the target is GC'ed we will be able to find and
                // remove the dead weak reference.
                this.HashCode = comparer.GetHashCode(key);
            }
        }

        internal sealed class WeakKeyComparer<T> : IEqualityComparer<object>
            where T : class
        {

            private IEqualityComparer<T> comparer;

            internal WeakKeyComparer(IEqualityComparer<T> comparer)
            {
                if (comparer == null)
                    comparer = EqualityComparer<T>.Default;

                this.comparer = comparer;
            }

            public int GetHashCode(object obj)
            {
                WeakKeyRef<T> weakKey = obj as WeakKeyRef<T>;
                if (weakKey != null) return weakKey.HashCode;
                return this.comparer.GetHashCode((T)obj);
            }

            // Note: There are actually 9 cases to handle here.
            //
            //  Let Wa = Alive Weak Reference
            //  Let Wd = Dead Weak Reference
            //  Let S  = Strong Reference
            //  
            //  x  | y  | Equals(x,y)
            // -------------------------------------------------
            //  Wa | Wa | comparer.Equals(x.Target, y.Target) 
            //  Wa | Wd | false
            //  Wa | S  | comparer.Equals(x.Target, y)
            //  Wd | Wa | false
            //  Wd | Wd | x == y
            //  Wd | S  | false
            //  S  | Wa | comparer.Equals(x, y.Target)
            //  S  | Wd | false
            //  S  | S  | comparer.Equals(x, y)
            // -------------------------------------------------
            public new bool Equals(object x, object y)
            {
                bool xIsDead, yIsDead;
                T first = GetTarget(x, out xIsDead);
                T second = GetTarget(y, out yIsDead);

                if (xIsDead)
                    return yIsDead ? x == y : false;

                if (yIsDead)
                    return false;

                return this.comparer.Equals(first, second);
            }

            private static T GetTarget(object obj, out bool isDead)
            {
                WeakKeyRef<T> wref = obj as WeakKeyRef<T>;
                T target;
                if (wref != null)
                {
                    target = wref.Target;
                    isDead = !wref.IsAlive;
                }
                else
                {
                    target = (T)obj;
                    isDead = false;
                }
                return target;
            }
        }

        #endregion
    }
}
