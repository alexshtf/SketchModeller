using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Union-find data structure where each set element is an integer in range [0..k] for some k.
    /// </summary>
    public class IndexedUnionFind
    {
        private readonly Entry[] entries;

        /// <summary>
        /// Constructs a new union-find data structure consisting of singleton sets only.
        /// </summary>
        /// <param name="count">The number of singleton sets to construct</param>
        public IndexedUnionFind(int count)
        {
            Contract.Requires(count >= 0);
            Contract.Ensures(Count == count);

            // create an array of entries, where the parent of each entry is itself and all ranks are zero.
            entries = new Entry[count];
            for (int i = 0; i < count; ++i)
                entries[i] = new Entry { Parent = i, Rank = 0 };
        }

        /// <summary>
        /// Finds a representative of an element's set.
        /// </summary>
        /// <param name="x">The element's index</param>
        /// <returns>An index of <paramref name="x"/>'s set representative element.</returns>
        [Pure]
        public int Find(int x)
        {
            Contract.Requires(0 <= x && x < Count);
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < Count);

            // find with path-compression
            if (entries[x].Parent == x)
                return x;
            else
            {
                // find with path-compression implementation
                entries[x].Parent = Find(entries[x].Parent);
                return entries[x].Parent;
            }
        }

        /// <summary>
        /// Unifies two sets
        /// </summary>
        /// <param name="x">An element of the first set to unify.</param>
        /// <param name="y">An element of the second set to unify.</param>
        public void Union(int x, int y)
        {
            Contract.Requires(0 <= x && x < Count);
            Contract.Requires(0 <= y && y < Count);
            Contract.Ensures(Find(x) == Find(y));

            x = Find(x);
            y = Find(y);

            if (x == y)
                return;

            // rank-based union
            if (entries[x].Rank < entries[y].Rank)
                entries[x].Parent = y;
            else if (entries[x].Rank > entries[y].Rank)
                entries[y].Parent = x;
            else
            {
                entries[y].Parent = x;
                entries[x].Rank = entries[x].Rank + 1;
            }
        }

        /// <summary>
        /// Gets the total number of elements in all sets
        /// </summary>
        public int Count
        {
            get { return entries.Length; }
        }

        private struct Entry
        {
            public int Parent;
            public int Rank;
        }
    }
}
