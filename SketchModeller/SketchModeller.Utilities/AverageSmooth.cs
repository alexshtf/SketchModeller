using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows;

namespace SketchModeller.Utilities
{
    public static class AverageSmooth
    {
        /// <summary>
        /// Performs a smoothing operation on an array of numbers such that each number
        /// gets closer to the average of the number before and after it.
        /// </summary>
        /// <param name="values">The input array of values</param>
        /// <param name="amount">The magnitude of the smoothing operation. 0 means no smoothing.
        /// 1 means maximal smoothing.</param>
        /// <returns>A smoothed array of numbers</returns>
        public static double[] Smooth(double[] values, double amount)
        {
            Contract.Requires(values != null);
            Contract.Requires(amount >= 0 && amount <= 1);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length != values.Length);

            var result = new double[values.Length];
            for (int i = 0; i < values.Length; ++i)
            {
                var prev = i > 0 ? values[i - 1] : values[i];
                var next = i < values.Length - 1 ? values[i + 1] : values[i];
                var avg = (prev + next) / 2;
                result[i] = (1 - amount) * values[i] + amount * avg;
            }

            return result;
        }

        /// <summary>
        /// Performs a smoothing operation on an array of numbers such that each number
        /// gets closer to the average of the number before and after it while keeping the first and
        /// last numbers unchanged.
        /// </summary>
        /// <param name="values">The input array of values</param>
        /// <param name="amount">The magnitude of the smoothing operation. 0 means no smoothing.
        /// 1 means maximal smoothing.</param>
        /// <returns>A smoothed array of numbers</returns>
        public static double[] SmoothKeepEdges(double[] values, double amount)
        {
            Contract.Requires(values != null);
            Contract.Requires(amount >= 0 && amount <= 1);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length != values.Length);

            var n = values.Length;
            if (n <= 2)
                return (double[])values.Clone();

            var result = new double[values.Length];
            for (int i = 1; i < values.Length - 1; ++i)
            {
                var prev = values[i - 1];
                var next = values[i + 1];
                var avg = (prev + next) / 2;
                result[i] = (1 - amount) * values[i] + amount * avg;
            }
            result[0] = values[0];
            result[result.Length - 1] = values[values.Length - 1];

            return result;
        }

        /// <summary>
        /// Performs a smoothing operation on 2D vectors
        /// </summary>
        /// <param name="vecs">The vectors to smooth</param>
        /// <param name="scalarSmooth">The scalar smoothing operation. It will be
        /// performed independently on X and Y coordinates.</param>
        /// <returns>A smoothed version of the vectors</returns>
        public static Vector[] SmoothVectors(Vector[] vecs, Func<double[], double[]> scalarSmooth)
        {
            Contract.Requires(vecs != null);
            Contract.Requires(scalarSmooth != null);
            Contract.Ensures(Contract.Result<Vector[]>() != null);
            Contract.Ensures(Contract.Result<Vector[]>().Length != vecs.Length);

            var x = vecs.Select(v => v.X).ToArray();
            var y = vecs.Select(v => v.Y).ToArray();

            x = scalarSmooth(x);
            y = scalarSmooth(y);

            var n = vecs.Length;
            var result =
                Enumerable.Range(0, n)
                .Select(i => new Vector(x[i], y[i]))
                .ToArray();

            return result;
        }

        /// <summary>
        /// Performs a smoothing operation on 2D points
        /// </summary>
        /// <param name="points">The vectors to smooth</param>
        /// <param name="scalarSmooth">The scalar smoothing operation. It will be
        /// performed independently on X and Y coordinates.</param>
        /// <returns>A smoothed version of the points</returns>
        public static Point[] SmoothPoints(Point[] points, Func<double[], double[]> scalarSmooth)
        {
            Contract.Requires(points != null);
            Contract.Requires(scalarSmooth != null);
            Contract.Ensures(Contract.Result<Point[]>() != null);
            Contract.Ensures(Contract.Result<Point[]>().Length != points.Length);

            var x = points.Select(p => p.X).ToArray();
            var y = points.Select(p => p.Y).ToArray();

            x = scalarSmooth(x);
            y = scalarSmooth(y);

            var n = points.Length;
            var result =
                Enumerable.Range(0, n)
                .Select(i => new Point(x[i], y[i]))
                .ToArray();

            return result;
        }
    }
}
