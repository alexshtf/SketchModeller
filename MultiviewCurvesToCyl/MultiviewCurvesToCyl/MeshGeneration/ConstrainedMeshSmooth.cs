using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Utils;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    static class ConstrainedMeshSmooth
    {
        /// <summary>
        /// Performs a smoothing step according to the paper "3D modelling with silhouettes" by Alec Rivers.
        /// </summary>
        /// <param name="positions">A list of positions for the mesh vertices</param>
        /// <param name="normals">A list of normals for the mesh vertices</param>
        /// <param name="triangleIndices"></param>
        /// <param name="constrainedIndices"></param>
        public static double Step(IList<Point3D> positions, IList<Vector3D> normals, MeshTopologyInfo topologyInfo, ISet<int> constrainedIndices)
        {
            Contract.Requires(positions != null);
            Contract.Requires(normals != null);
            Contract.Requires(topologyInfo != null);
            Contract.Requires(constrainedIndices != null);
            Contract.Requires(positions.Count == normals.Count);
            Contract.Requires(Contract.ForAll(constrainedIndices, index => index < positions.Count));

            Contract.Ensures(Contract.OldValue(positions.Count) == positions.Count); // we didn't change the number of vertices
            Contract.Ensures(Contract.OldValue(normals.Count) == normals.Count); // we didn't change the number of vertex normals

            // we don't change the position of constrained vertices.
            //Contract.Ensures(Contract.ForAll(constrainedIndices, index => Contract.OldValue(positions[index]) == positions[index]));

            var currentCurvatures =
                (from item in positions.ZipIndex()
                 let x = item.Value
                 let i = item.Index
                 let neighbors = from neighborIndex in topologyInfo.VertexNeighborsOfVertex(i) 
                                 select positions[neighborIndex]
                 let laplacian = x - neighbors.Centroid()
                 let normal = normals[i].Normalized()
                 select Vector3D.DotProduct(laplacian, normal))
                 .ToArray();

            // target curvatures are averages of curvatures at neighboring vertices - smoothed original curvatures
            var targetCurvatures =
                from item in currentCurvatures.ZipIndex()
                let curvature = item.Value
                let index = item.Index
                let neighborCurvatures = from neighborIndex in topologyInfo.VertexNeighborsOfVertex(index) 
                                         select currentCurvatures[neighborIndex]
                select neighborCurvatures.Average();

            // new positions are calculated such that the new curvatures will be closer to target curvatures
            var newPositions =
                from item in positions.Zip(targetCurvatures).ZipIndex()
                let index = item.Index
                let x = item.Value.Item1
                let cPrime = item.Value.Item2
                let n = normals[index]
                let neighborsAvg = (from neighborIndex in topologyInfo.VertexNeighborsOfVertex(index) 
                                    select positions[neighborIndex]
                                   ).Centroid()
                select neighborsAvg + n.Normalized() * cPrime;

            // we will later modify the positions and normals, so we want to execute all LINQ queries that depend on them.
            newPositions = newPositions.ToArray(); 

            // sum of distances the vertices moved from old positions to new positions
            var totalChange =
                (from tuple in positions.Zip(newPositions)
                 let oldPos = tuple.Item1
                 let newPos = tuple.Item2
                 select (oldPos - newPos).Length
                ).Sum();

            // write values to new positions of non-constrained vertices
            foreach (var tuple in newPositions.ZipIndex())
            {
                var idx = tuple.Index;
                var newPos = tuple.Value;

                if (!constrainedIndices.Contains(idx))
                    positions[idx] = newPos;
            }

            RecalculateNormals(positions, normals, topologyInfo);

            return totalChange;
        }

        private static void RecalculateNormals(IList<Point3D> positions, IList<Vector3D> normals, MeshTopologyInfo topologyInfo)
        {
            Contract.Requires(positions.Count == normals.Count);
            Contract.Requires(positions.Count <= topologyInfo.VertexCount);

            // calculate normals for all triangles
            var triangleNormals = 
                (from index in System.Linq.Enumerable.Range(0, topologyInfo.TriangleCount)
                 let triangle = topologyInfo.Triangles(index)
                 let p1 = positions[triangle.Item1]
                 let p2 = positions[triangle.Item2]
                 let p3 = positions[triangle.Item3]
                 let normal = Plane3D.FromPoints(p1, p2, p3).Normal
                 select normal)
                .ToArray();
            
            // the new normal at each vertex is the average of normals of its adjacent triangles
            for (int i = 0; i < positions.Count; ++i)
            {
                var neighborTriangleNormals = 
                    (from neighborTriangleIndex in topologyInfo.TriangleNeighborsOfVertex(i)
                     select triangleNormals[neighborTriangleIndex])
                    .ToList();

                Contract.Assume(neighborTriangleNormals.Count > 0);
                var averageNormal =
                    neighborTriangleNormals.Aggregate(MathUtils3D.ZeroVector, (x, y) => x + y) / neighborTriangleNormals.Count;

                normals[i] = averageNormal;
            }
        }
    }
}
