using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Accord.Math.Decompositions;
using Accord.Math;
using Meta.Numerics.Matrices;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// A class that performs ellipse fitting
    /// </summary>
    public static class EllipseFitter
    {
        /// <summary>
        /// Samples points along the ellipse that best fits the given points.
        /// </summary>
        /// <param name="points">The points that approximate an ellipse</param>
        /// <param name="count">The number of sample points on the best-fit ellipse to create</param>
        /// <returns>An array of <paramref name="count"/> poins sampled on the ellipse that best fits <paramref name="points"/>,</returns>
        public static Point[] Sample(IList<Point> points, int count = 20)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Count > 6);
            Contract.Requires(count > 0);
            Contract.Ensures(Contract.Result<Point[]>() != null);
            Contract.Ensures(Contract.Result<Point[]>().Length == count);
            
            var ellipse = Fit(points);
            var result = new Point[count];
            var rotMatrix = new RotateTransform(ellipse.Degrees).Value;
            for (int i = 0; i < count; ++i)
            {
                var t = 2 * Math.PI * i / (double)(count - 1);
                var vec = new Vector(ellipse.XRadius * Math.Cos(t), ellipse.YRadius * Math.Sin(t));
                result[i] = ellipse.Center + rotMatrix.Transform(vec);
            }
            return result;
        }

        public static Point[] Sample(IList<Point> points, EllipseParams ellipse, int count = 20)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Count > 6);
            Contract.Requires(count > 0);
            Contract.Ensures(Contract.Result<Point[]>() != null);
            Contract.Ensures(Contract.Result<Point[]>().Length == count);
            /*using (StreamWriter writer = File.CreateText("Points.txt"))
            {
                foreach (Point pnt in points)
                {
                    writer.Write(pnt.X + " " + pnt.Y);
                    writer.Write(writer.NewLine);
                }
            }
            MessageBox.Show("Done Writing Points");*/
            /*EllipseParams ellipse = new EllipseParams
            {
                Center = new Point(0.9566, 0.8023),
                XRadius = 0.1149,
                YRadius = 0.0894,
                Degrees = -1.2418 * 180 / Math.PI
            };*/
            var result = new Point[count];
            var rotMatrix = new RotateTransform(ellipse.Degrees).Value;
            for (int i = 0; i < count; ++i)
            {
                var t = 2 * Math.PI * i / (double)(count - 1);
                var vec = new Vector(ellipse.XRadius * Math.Cos(t), ellipse.YRadius * Math.Sin(t));
                result[i] = ellipse.Center + rotMatrix.Transform(vec);
            }
            return result;
        }

        /// <summary>
        /// Performs ellipse fitting according to the method described in "Direct Least-Squares Fitting of Ellipses"
        /// by Fitzgibbon et al. 
        /// </summary>
        /// <param name="points">The list of the ellipse's points</param>
        /// <returns></returns>
        public static EllipseParams Fit(IList<Point> points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Count >= 6);
            Contract.Ensures(Contract.Result<EllipseParams>().XRadius >= 0);
            Contract.Ensures(Contract.Result<EllipseParams>().YRadius >= 0);

            // construct the design matrix parts
            var d1 = new RectangularMatrix(points.Count, 3);
            var d2 = new RectangularMatrix(points.Count, 3);

            for (int i = 0; i < points.Count; ++i)
            {
                d1[i, 0] = points[i].X * points[i].X;
                d1[i, 1] = points[i].X * points[i].Y;
                d1[i, 2] = points[i].Y * points[i].Y;

                d2[i, 0] = points[i].X;
                d2[i, 1] = points[i].Y;
                d2[i, 2] = 1;
            }

            var s1 = MultiplyTranspose(d1, d1);
            var s2 = MultiplyTranspose(d1, d2);
            var s3 = MultiplyTranspose(d2, d2);

            var c1 = new SquareMatrix(3);
            c1[0, 2] = 2;
            c1[1, 1] = -1;
            c1[2, 0] = 2;

            var m = c1.Inverse() * (s1 - s2 * s3.Inverse() * s2.Transpose());

            ColumnVector a1 = null;
            var eigen = m.Eigensystem();
            for (int i = 0; i < eigen.Dimension; ++i)
            {
                var evec = eigen.Eigenvector(i);
                var cond = 4 * evec[0].Re * evec[2].Re - evec[1].Re * evec[1].Re;
                if (cond > 0)
                    a1 = new ColumnVector(evec[0].Re, evec[1].Re, evec[2].Re);
            }
            Debug.Assert(a1 != null);
            var a2 = -s3.Inverse() * s2.Transpose() * a1;

            var conic = a1.Concat(a2).ToArray();
            var ellipseParams = Conic2Parametric(conic);
            return ellipseParams;
        }

        /// <summary>
        /// Converts conic representation of an ellipse (given the coefficients vector) to the parametric representation.
        /// </summary>
        /// <param name="coefficients">The implicit equation coefficients</param>
        /// <returns>Parametric representation</returns>
        private static EllipseParams Conic2Parametric(double[] conic)
        {
            var A = new SymmetricMatrix(2);
            A[0, 0] = conic[0];                 // a
            A[1, 1] = conic[2];                 // c
            A[0, 1] = A[1, 0] = 0.5 * conic[1]; // half b

            var B = new ColumnVector(conic[3], conic[4]);
            var C = conic[5];

            var eig = A.Eigensystem();
            Debug.Assert(eig.Eigenvalue(0) * eig.Eigenvalue(1) > 0);

            var D = new double[] { eig.Eigenvalue(0), eig.Eigenvalue(1) };
            var Q = eig.Eigentransformation().Transpose();
            var t = -0.5 * A.Inverse() * B;

            var c_h = t.Transpose() * A * t + B.Transpose() * t + C;

            return new EllipseParams
            {
                Center = new Point(t[0], t[1]),
                XRadius = Math.Sqrt(-c_h / D[0]),
                YRadius = Math.Sqrt(-c_h / D[1]),
                Degrees = 180 * Math.Atan2(Q[0, 1], Q[0, 0]) / Math.PI,
            };

        }

        private static SquareMatrix MultiplyTranspose(RectangularMatrix m1, RectangularMatrix m2)
        {
            var resultRectangular = m1.Transpose() * m2;
            return ToSquareMatrix(resultRectangular);
        }

        private static SquareMatrix ToSquareMatrix(RectangularMatrix source)
        {
            Contract.Requires(source != null);
            Contract.Requires(source.RowCount == source.ColumnCount);
            Contract.Ensures(Contract.Result<SquareMatrix>() != null);
            Contract.Ensures(Contract.Result<SquareMatrix>().Dimension == source.RowCount);

            var n = source.RowCount;
            var result = new SquareMatrix(n);
            for (int row = 0; row < n; ++row)
                for (int col = 0; col < n; ++col)
                    result[row, col] = source[row, col];

            return result;
        }
    }

    /// <summary>
    /// Described the ellipse parameters
    /// </summary>
    public struct EllipseParams
    {
        public Point Center;
        public double Degrees;
        public double XRadius;
        public double YRadius;
    }
}
