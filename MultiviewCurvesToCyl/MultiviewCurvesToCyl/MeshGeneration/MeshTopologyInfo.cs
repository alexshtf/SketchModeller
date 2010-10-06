using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Utils;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    class MeshTopologyInfo
    {
        private readonly int vertexCount;
        private readonly List<Tuple<int, int, int>> triangles;
        private readonly int[][] vertexVertexNeighbors;
        private readonly int[][] vertexTriangleNeighbors;
        private readonly HashSet<EdgeInfo> edges;
        private readonly HashSet<EdgeInfo> boundaryEdges;
        private readonly HashSet<int> boundaryVertices;

        public MeshTopologyInfo(IEnumerable<Tuple<int, int, int>> triangles)
        {
            Contract.Requires(triangles != null);
            Contract.Requires(triangles.Count() > 0);
            Contract.Requires(Contract.ForAll(triangles, triangle => IsValidTriangle(triangle)));
            Contract.Requires(HasDuplicateTriangles(triangles) == false);

            this.triangles = triangles.ToList();
            vertexCount = triangles.SelectMany(x => new int[] { x.Item1, x.Item2, x.Item3}).Max() + 1;
            vertexVertexNeighbors = GetVertexVertexNeighbors(vertexCount, triangles);
            vertexTriangleNeighbors = GetVertexTriangleNeighbors(vertexCount, triangles);
            edges = GetEdges(triangles);
            boundaryEdges = GetBoundaryEdges(edges, triangles);
            boundaryVertices = GetBoundaryVertices(boundaryEdges);
        }

        public int VertexCount
        {
            get { return vertexCount; }
        }

        public int TriangleCount
        {
            get { return triangles.Count; }
        }

        public Tuple<int, int, int> Triangles(int index)
        {
            return triangles[index];
        }

        public IEnumerable<int> VertexNeighborsOfVertex(int vertexIndex)
        {
            return new ReadOnlyCollection<int>(vertexVertexNeighbors[vertexIndex]);
        }

        public IEnumerable<int> TriangleNeighborsOfVertex(int vertexIndex)
        {
            return new ReadOnlyCollection<int>(vertexTriangleNeighbors[vertexIndex]);
        }

        public bool IsBoundaryVertex(int index)
        {
            return boundaryVertices.Contains(index);
        }

        public bool IsBoundaryEdge(int v1, int v2)
        {
            var edgeInfo = new EdgeInfo(v1, v2);
            return boundaryEdges.Contains(edgeInfo);
        }

        public bool IsEdge(int v1, int v2)
        {
            var edgeInfo = new EdgeInfo(v1, v2);
            var isEdge = edges.Contains(edgeInfo);
            return isEdge;
        }

        [Pure]
        public static bool IsValidTriangle(Tuple<int, int, int> triangle)
        {
            var result =
                triangle.Item1 >= 0 &&
                triangle.Item2 >= 0 &&
                triangle.Item3 >= 0 &&
                triangle.Item1 != triangle.Item2 &&
                triangle.Item2 != triangle.Item3 &&
                triangle.Item1 != triangle.Item3;
            return result;
        }

        [Pure]
        public static bool HasDuplicateTriangles(IEnumerable<Tuple<int, int, int>> triangles)
        {
            var set = new HashSet<TriangleInfo>();
            foreach (var triangle in triangles)
            {
                var triangleInfo = new TriangleInfo(triangle);

                if (set.Contains(triangleInfo))
                    return true;
                set.Add(triangleInfo);
            }

            return false;
        }

        #region private methods

        private static HashSet<int> GetBoundaryVertices(HashSet<EdgeInfo> boundaryEdges)
        {
            var result = new HashSet<int>();
            foreach (var item in boundaryEdges)
            {
                result.Add(item.FirstVertex);
                result.Add(item.SecondVertex);
            }

            return result;
        }

        private static HashSet<EdgeInfo> GetBoundaryEdges(IEnumerable<EdgeInfo> edges, IEnumerable<Tuple<int, int, int>> triangles)
        {
            Contract.Requires(edges != null);
            Contract.Requires(triangles != null);
            Contract.Ensures(Contract.Result<HashSet<EdgeInfo>>().IsSubsetOf(edges));

            // edge to triangles-count dictionary initially stores 0 triangles for each edge. We will increase this number
            // as we enumerate the triangles.
            var edgeToTriCount = edges.ToDictionary(x => x, x => 0);
            foreach (var triangle in triangles)
            {
                // create edge-info for all edges of this triangle
                var e1 = new EdgeInfo(triangle.Item1, triangle.Item2);
                var e2 = new EdgeInfo(triangle.Item2, triangle.Item3);
                var e3 = new EdgeInfo(triangle.Item3, triangle.Item1);

                // verify that the above edges are "ok"
                Contract.Assume(edges.Contains(e1));
                Contract.Assume(edges.Contains(e2));
                Contract.Assume(edges.Contains(e3));

                // add a triangle to all of the above edges
                edgeToTriCount[e1] = edgeToTriCount[e1] + 1;
                edgeToTriCount[e2] = edgeToTriCount[e2] + 1;
                edgeToTriCount[e3] = edgeToTriCount[e3] + 1;
            }

            var result = from pair in edgeToTriCount
                         where pair.Value < 2
                         select pair.Key;

            return new HashSet<EdgeInfo>(result);
        }

        private static HashSet<EdgeInfo> GetEdges(IEnumerable<Tuple<int, int, int>> triangles)
        {
            var edges = new HashSet<EdgeInfo>();

            foreach (var triangleInfo in triangles)
            {
                var i = triangleInfo.Item1;
                var j = triangleInfo.Item2;
                var k = triangleInfo.Item3;

                edges.Add(new EdgeInfo(i, j));
                edges.Add(new EdgeInfo(j, k));
                edges.Add(new EdgeInfo(i, k));
            }

            return edges;
        }

        private static int[][] GetVertexTriangleNeighbors(int vertexCount, IEnumerable<Tuple<int, int, int>> triangles)
        {
            var neighborhoodSets = new HashSet<int>[vertexCount];
            for (int i = 0; i < neighborhoodSets.Length; i++)
                neighborhoodSets[i] = new HashSet<int>();

            foreach (var item in triangles.ZipIndex())
            {
                // index of the triangle
                var triangleIndex = item.Index;

                // indices of triangle vertices
                var v1 = item.Value.Item1;
                var v2 = item.Value.Item2;
                var v3 = item.Value.Item3;

                neighborhoodSets[v1].Add(triangleIndex);
                neighborhoodSets[v2].Add(triangleIndex);
                neighborhoodSets[v3].Add(triangleIndex);
            }

            return neighborhoodSets.Select(set => set.ToArray()).ToArray();
        }

        private int[][] GetVertexVertexNeighbors(int vertexCount, IEnumerable<Tuple<int, int, int>> triangleIndices)
        {
            // we build neighborhood information usung sets. Sets are used to avoid duplicates
            var neighborhoodSets = new HashSet<int>[vertexCount];
            for (int i = 0; i < neighborhoodSets.Length; i++)
                neighborhoodSets[i] = new HashSet<int>();
            foreach (var triple in triangleIndices)
            {
                var i = triple.Item1;
                var j = triple.Item2;
                var k = triple.Item3;

                // this is a triangle - so every point is a neighbor of the other two
                neighborhoodSets[i].Add(j); neighborhoodSets[i].Add(k);
                neighborhoodSets[j].Add(i); neighborhoodSets[j].Add(k);
                neighborhoodSets[k].Add(i); neighborhoodSets[k].Add(j);
            }

            // we convert each set to an array to get the final adjacency info.
            var neighborsInfo = neighborhoodSets.Select(set => set.ToArray()).ToArray();

            return neighborsInfo;
        }

        #endregion
            
        #region EdgeInfo class

        private class EdgeInfo : IEquatable<EdgeInfo>
        {
            private static readonly EqualityComparer<int> DefaultComparer =
                EqualityComparer<int>.Default;

            private readonly int lowIdx;
            private readonly int highIdx;

            public EdgeInfo(int v1, int v2)
            {
                Contract.Requires(v1 >= 0);
                Contract.Requires(v2 >= 0);
                Contract.Requires(v1 != v2);

                lowIdx = Math.Min(v1, v2);
                highIdx = Math.Max(v1, v2);
            }

            public int FirstVertex
            {
                get { return lowIdx; }
            }

            public int SecondVertex
            {
                get { return highIdx; }
            }

            public bool Equals(EdgeInfo other)
            {
                return 
                    other.lowIdx == this.lowIdx &&
                    other.highIdx == this.highIdx;
            }

            public override int GetHashCode()
            {
                var h1 = DefaultComparer.GetHashCode(lowIdx);
                var h2 = DefaultComparer.GetHashCode(highIdx);

                return ((h1 << 5) + h1) ^ h2;
            }
        }

        #endregion

        #region TriangleInfo Class

        private class TriangleInfo : IEquatable<TriangleInfo>
        {
            private static readonly EqualityComparer<int> DefaultComparer =
                EqualityComparer<int>.Default;

            private readonly int low;
            private readonly int mid;
            private readonly int high;

            public TriangleInfo(Tuple<int, int, int> triangleInfo)
            {
                Contract.Requires(MeshTopologyInfo.IsValidTriangle(triangleInfo));

                var vertices = new int[] { triangleInfo.Item1, triangleInfo.Item2, triangleInfo.Item3 };
                Array.Sort(vertices);

                low  = vertices[0];
                mid  = vertices[1];
                high = vertices[2];
            }

            public bool Equals(TriangleInfo other)
            {
                return
                    this.low == other.low &&
                    this.mid == other.mid &&
                    this.high == other.high;
            }

            public override int GetHashCode()
            {
                var h1 = DefaultComparer.GetHashCode(low);
                var h2 = DefaultComparer.GetHashCode(mid);
                var h3 = DefaultComparer.GetHashCode(high);

                var tmp = ((h1 << 5) + h1) ^ h2;
                var final = ((tmp << 5) + tmp) ^ h3;

                return final;
            }
        }

        #endregion
    }
}
