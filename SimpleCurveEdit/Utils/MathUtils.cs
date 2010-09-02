using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// Estimates curvature at the middle of 3 points that reside on a quadratic curve tha passes through the three points.
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        /// <returns>The estimated curvature of <paramref name="p2"/> on the curve.</returns>
        public static double QuadraticCurvatureEstimate(Point p1, Point p2, Point p3)
        {
            var curveFit = QuadraticFit(p1, p2, p3);

            var xd = curveFit.Item1.Y; var yd = curveFit.Item2.Y;           // 1st derivative at t = 0
            var xdd = 2 * curveFit.Item1.X; var ydd = 2 * curveFit.Item2.X; // 2nd derivative at t = 0

            var dominator = xd * ydd - yd * xdd;
            var demominator = Math.Pow(xd * xd + yd * yd, 1.5);

            return dominator / demominator;
        }

        /// <summary>
        /// Fits a quadratic parametric curve to three points.
        /// </summary>
        /// <param name="p1">first point</param>
        /// <param name="p2">second point</param>
        /// <param name="p3">third point</param>
        /// <returns>The coefficients of the parametric curve that passes through the given three points. See remarks.</returns>
        /// <remarks>
        /// A quadratic parametric curve C(t) = (x(t), y(t)) can be written as:
        ///     x(t) = at² + bt + c
        ///     y(t) = dt² + et + f
        /// This function estimates the parameters a, b, c, d, e, f such that C(-1) = p1, C(0) = p2, C(1) = p3. It returns
        /// a tuple of two vectors. The first vector contains (a, b, c) and the second contains (d, e, f).
        /// </remarks>
        public static Tuple<Vector3D, Vector3D> QuadraticFit(Point p1, Point p2, Point p3)
        {
            var x1 = p1.X; var y1 = p1.Y;
            var x2 = p2.X; var y2 = p2.Y;
            var x3 = p3.X; var y3 = p3.Y;

            // from MATLAB. Inverse of the matrix in a linear equation solving for the solution
            double[,] Ainv = {
                                 {  1.5000,       0, -1.0000,         0,         0,         0,       },
                                 {       0,       0,       0,         0,    0.5000,         0,       },
                                 { -1.0000,       0,  1.0000,         0,         0,         0,       },
                                 {       0,  1.5000,       0,   -1.0000,         0,         0,       },
                                 {       0,       0,       0,         0,         0,    0.5000,       },
                                 {       0, -1.0000,       0,    1.0000,         0,         0,       },
                             };

            double[] b = {   x1 + x3, 
                             y1 + y3,
                             x1 + x2 + x3,
                             y1 + y2 + y3,
                             x3 - x1,
                             y3 - y1,
                         };

            double[] c = new double[b.Length];
            for (int i = 0; i < c.Length; ++i)
            {
                for (int j = 0; j < c.Length; ++j)
                    c[i] += Ainv[i, j] * b[j];
            }

            return Tuple.Create(
                new Vector3D(c[0], c[1], c[2]), 
                new Vector3D(c[3], c[4], c[5]));
        }
    }
}
