using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Accord.Math;
using Accord.Math.Decompositions;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows.Media;

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

            // we have a constant constraints matrix describing the constraint 4ac-b² = 1
            var constraintMatrix = new double[6, 6];
            constraintMatrix[0, 2] = 2;
            constraintMatrix[1, 1] = -1;
            constraintMatrix[2, 0] = 2;

            // a data matrix - each row is [x² xy y² x y 1] for each data point (x, y)
            var dataMatrix = new double[points.Count, 6];
            for (int row = 0; row < points.Count; ++row)
            {
                var x = points[row].X;
                var y = points[row].Y;
                dataMatrix[row, 0] = x * x;
                dataMatrix[row, 1] = x * y;
                dataMatrix[row, 2] = y * y;
                dataMatrix[row, 3] = x;
                dataMatrix[row, 4] = y;
                dataMatrix[row, 5] = 1;
            }

            // The scatter matrix - D^T * D
            var scatterMatrix = dataMatrix.Transpose().Multiply(dataMatrix);

            // get the eigenvector of the only positive eigenvalue of the scatter matrix - this is the 
            // ellipse's equation coefficients.
            var eigen = new GeneralizedEigenvalueDecomposition(scatterMatrix, constraintMatrix);
            int positiveEigIndex = -1;
            for (int i = 0; i < 6; ++i)
                if (eigen.RealEigenvalues[i] > 0 && !double.IsInfinity(eigen.RealEigenvalues[i]))
                {
                    Debug.Assert(positiveEigIndex == -1);
                    positiveEigIndex = i;
                }
            var coefficients = eigen.Eigenvectors.GetColumn(positiveEigIndex);

            // extract ellipse parameters from the coefficients.
            var ellipseParams = Conic2Parametric(coefficients);
            return ellipseParams;
        }

        /// <summary>
        /// Converts conic representation of an ellipse (given the coefficients vector) to the parametric representation.
        /// </summary>
        /// <param name="coefficients">The implicit equation coefficients</param>
        /// <returns>Parametric representation</returns>
        private static EllipseParams Conic2Parametric(double[] coefficients)
        {
            var A = new double[2, 2];
            A[0, 0] = coefficients[0];                 // a
            A[1, 1] = coefficients[2];                 // c
            A[0, 1] = A[1, 0] = 0.5 * coefficients[1]; // half b

            var B = new double[] { coefficients[3], coefficients[4] };
            var C = coefficients[5];

            var eig = new EigenvalueDecomposition(A);
            Debug.Assert(eig.RealEigenvalues.Product() > 0);

            var D = eig.RealEigenvalues;
            var Q = eig.Eigenvectors.Transpose();
            var t = A.Solve(B).Multiply(-0.5);

            var c_h = t.Multiply(A).InnerProduct(t) + B.InnerProduct(t) + C;

            return new EllipseParams
            {
                Center = new Point(t[0], t[1]),
                XRadius = Math.Sqrt(-c_h / D[0]),
                YRadius = Math.Sqrt(-c_h / D[1]),
                Degrees = 180 * Math.Atan2(Q[0, 1], Q[0, 0]) / Math.PI,
            };

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
