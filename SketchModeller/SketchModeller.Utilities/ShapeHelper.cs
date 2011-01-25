using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Utils;

namespace SketchModeller.Utilities
{
    public static class ShapeHelper
    {
        /// <summary>
        /// Generates an approximation of a circle given its center, two orthonormal basis vectors for the plane, and its radius.
        /// </summary>
        /// <param name="center">The circle's center</param>
        /// <param name="u">First basis vector. This is the "X" axis.</param>
        /// <param name="v">Second basis vector. This is the "Y" axis.</param>
        /// <param name="radius">Circle's radius.</param>
        /// <param name="count">Number of points that approximate the circle</param>
        /// <returns>An array of points that form the circle's approximation</returns>
        public static Point3D[] GenerateCircle(Point3D center, Vector3D u, Vector3D v, double radius, int count)
        {
            Contract.Requires(MathUtils3D.AreOrthogonal(u, v));
            Contract.Requires(Math.Abs(u.LengthSquared - 1) < 10 * NumericUtils.DBL_MACHINE_EPSILON);
            Contract.Requires(Math.Abs(v.LengthSquared - 1) < 10 * NumericUtils.DBL_MACHINE_EPSILON);
            Contract.Requires(radius > 0);
            Contract.Requires(count >= 3);
            Contract.Ensures(Contract.Result<Point3D[]>() != null);
            Contract.Ensures(Contract.Result<Point3D[]>().Length == count);

            var result = new Point3D[count];

            for (int i = 0; i < count; ++i)
            {
                var fraction = i / (double)count;
                var angle = 2 * Math.PI * fraction;
                result[i] = center + radius * (Math.Cos(angle) * u + Math.Sin(angle) * v);
            }
            return result;
        }
    }
}
