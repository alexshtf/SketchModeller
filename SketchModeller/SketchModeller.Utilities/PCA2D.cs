using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;
using Accord.Math.Decompositions;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Performs two-dimensional principal component analysis
    /// </summary>
    public static class PCA2D
    {
        /// <summary>
        /// Computes PCA of a set of 2D points.
        /// </summary>
        /// <param name="points">The points collection</param>
        /// <returns>A triple containing the centroid of the points set and two principal axes
        /// best representing the points set.</returns>
        public static Tuple<Point, Vector, Vector> PCA(this IEnumerable<Point> points)
        {
            var ptsArray = points.ToArray();

            var count = ptsArray.Length;
            var xAvg = points.Select(p => p.X).Average();
            var yAvg = points.Select(p => p.Y).Average();

            double[,] mat = new double[2, count];
            foreach (var i in Enumerable.Range(0, count))
            {
                mat[0, i] = ptsArray[i].X - xAvg;
                mat[1, i] = ptsArray[i].Y - yAvg;
            }

            var svd = new SingularValueDecomposition(mat, true, false); // we need only left singular vectors
            var v1 = new Vector(svd.LeftSingularVectors[0, 0], svd.LeftSingularVectors[1, 0]);
            var v2 = new Vector(svd.LeftSingularVectors[0, 1], svd.LeftSingularVectors[1, 1]);
            return Tuple.Create(new Point(xAvg, yAvg), v1, v2);
        }
    }
}
