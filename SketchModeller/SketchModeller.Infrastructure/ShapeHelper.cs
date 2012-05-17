using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Utils;
using System.Windows;

namespace SketchModeller.Infrastructure
{
    public static class ShapeHelper
    {
        /// <summary>
        /// Projects a 3D curve to 2D using the standard orthographic projection used in this application.
        /// </summary>
        /// <param name="curve">The 3D curve to project</param>
        /// <returns></returns>
        public static Point[] ProjectCurve(params Point3D[] curve)
        {
            Contract.Requires(curve != null);
            Contract.Ensures(Contract.Result<Point[]>() != null);
            Contract.Ensures(Contract.Result<Point[]>().Length == curve.Length);

            var result = new Point[curve.Length];
            for (int i = 0; i < curve.Length; ++i)
            {
                result[i].X = curve[i].X;
                result[i].Y = -curve[i].Y;
            }

            return result;
        }

        /// <summary>
        /// Generates the points of a rectangle given its parameters.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="normal"></param>
        /// <param name="widthVector"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Point3D[] GenerateRectangle(Point3D center, Vector3D normal, Vector3D widthVector, double width, double height)
        {
        }

        /// <summary>
        /// Generates an approximation of a circle given its center, a normal vector for the plane, and its radius.
        /// </summary>
        /// <param name="center">The circle's center</param>
        /// <param name="normal">Normal vector to the circle's plane</param>
        /// <param name="radius">Circle's radius.</param>
        /// <param name="count">Number of points that approximate the circle</param>
        /// <returns>An array of points that form the circle's approximation</returns>
        public static Point3D[] GenerateCircle(Point3D center, Vector3D normal, double radius, int count)
        {
            Contract.Requires(radius > 0);
            Contract.Requires(count >= 3);
            Contract.Ensures(Contract.Result<Point3D[]>() != null);
            Contract.Ensures(Contract.Result<Point3D[]>().Length == count);
            Contract.Ensures(Contract.ForAll(
                Contract.Result<Point3D[]>(),
                pnt => NumericUtils.AlmostEqual(radius * radius, (pnt - center).LengthSquared, 100)));

            var xAxis = MathUtils3D.NormalVector(normal).Normalized();
            var yAxis = Vector3D.CrossProduct(normal, xAxis).Normalized();

            return GenerateCircle(center, xAxis, yAxis, radius, count);
        }

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
            Contract.Requires(NumericUtils.AlmostEqual(u.LengthSquared, 1, 10));
            Contract.Requires(NumericUtils.AlmostEqual(v.LengthSquared, 1, 10));
            Contract.Requires(radius > 0);
            Contract.Requires(count >= 3);
            Contract.Ensures(Contract.Result<Point3D[]>() != null);
            Contract.Ensures(Contract.Result<Point3D[]>().Length == count);
            // we cannot really ensure that (?)
            //Contract.Ensures(Contract.ForAll(
            //    Contract.Result<Point3D[]>(), 
            //    pnt => NumericUtils.AlmostEqual((pnt - center).LengthSquared, radius * radius, 100)));

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
