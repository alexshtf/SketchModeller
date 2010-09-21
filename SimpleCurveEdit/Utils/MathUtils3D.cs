using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class MathUtils3D
    {
        private const double EPSILON = 1E-5;

        public static readonly Vector3D ZeroVector = new Vector3D();

        public static readonly Point3D Origin = new Point3D();
        public static readonly Vector3D UnitX = new Vector3D(1, 0, 0);
        public static readonly Vector3D UnitY = new Vector3D(0, 1, 0);
        public static readonly Vector3D UnitZ = new Vector3D(0, 0, 1);

        [Pure]
        public static Vector3D Normalized(this Vector3D v)
        {
            v.Normalize();
            return v;
        }

        [Pure]
        public static Point3D Centroid(this IEnumerable<Point3D> points)
        {
            Contract.Requires(points != null);

            var vectors = from pnt in points
                          select pnt - Origin;
            var average = vectors.Aggregate(new Vector3D(), (x, y) => x + y) / (double)vectors.Count();

            return Origin + average;
        }

        /// <summary>
        /// Finds a normalized vector that is most similar, in the inner-product sense, to a specified vector and is orthogonal
        /// to another vector.
        /// </summary>
        /// <param name="similarTo">The vector to which we find the most similar result</param>
        /// <param name="orthogonalTo">The vector to which the result will be orthogonal</param>
        /// <returns>A vector that is orthogonal to <paramref name="orthogonalTo"/>, normalized, and most similar in the 
        /// inner product sense to <paramref name="similarTo"/>.</returns>
        public static Vector3D MostSimilarPerpendicular(Vector3D similarTo, Vector3D orthogonalTo)
        {
            Contract.Requires(!AreParallel(similarTo, orthogonalTo));
            Contract.Ensures(Math.Abs(Vector3D.DotProduct(orthogonalTo, Contract.Result<Vector3D>())) < EPSILON); // the result is really orthogonal

            orthogonalTo.Normalize();

            var normalizer = 1 / Vector3D.CrossProduct(similarTo, orthogonalTo).Length;
            var dot = Vector3D.DotProduct(similarTo, orthogonalTo);

            return normalizer * new Vector3D(
                similarTo.X - orthogonalTo.X * dot,
                similarTo.Y - orthogonalTo.Y * dot,
                similarTo.Z - orthogonalTo.Z * dot);
        }

        [Pure]
        public static bool AreOrthogonal(Vector3D v1, Vector3D v2)
        {
            if (v1.LengthSquared > 0)
                v1.Normalize();

            if (v2.LengthSquared > 0)
                v2.Normalize();

            var dot = Vector3D.DotProduct(v1, v2);

            return Math.Abs(dot) < EPSILON;
        }

        [Pure]
        public static bool AreParallel(Vector3D v1, Vector3D v2)
        {
            Contract.Requires(v1 != ZeroVector);
            Contract.Requires(v2 != ZeroVector);

            v1.Normalize();
            v2.Normalize();
            var cross = Vector3D.CrossProduct(v1, v2);

            return cross.LengthSquared < EPSILON;
        }
    }
}
