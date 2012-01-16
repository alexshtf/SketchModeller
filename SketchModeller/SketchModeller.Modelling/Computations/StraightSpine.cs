using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;
using SketchModeller.Utilities;

using Enumerable = System.Linq.Enumerable;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Computations
{
    static class StraightSpine
    {
        /// <summary>
        /// Computes a straight spine approximation given a single silhouette line.
        /// </summary>
        /// <param name="pts">Polyline defining one of the silhouette lines</param>
        /// <param name="progress">The progress points along the spine for which to compute the radii</param>
        /// <param name="onSpine">A point on the spine</param>
        /// <param name="spineDirection">The direction vector of the spine</param>
        /// <returns>
        /// A tuple (radii, start, end). <c>radii</c> is an array of the radii along the spine points defined by <paramref name="progress"/>. 
        /// <c>start</c> is the spine's starting point and <c>end</c> is the spine's ending point.
        /// </returns>
        public static Tuple<double[], Point, Point> Compute(Point[] pts, double[] progress, Point onSpine, Vector spineDirection)
        {
            Contract.Requires(pts != null);
            Contract.Requires(progress != null && progress.Length >= 2); // we need at-least two spine pints
            Contract.Requires(progress.First() == 0 && progress.Last() == 1);

            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>() != null);
            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>().Item1 != null);
            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>().Item1.Length == progress.Length);

            var spineProjections =
                from pnt in pts
                let t = pnt.ProjectOnLine(onSpine, spineDirection)
                select t;

            spineProjections = spineProjections.ToArray();
            var progressProjections = ComputeProgressProjections(progress, spineProjections);

            var ptsDistanceFunc = DistanceFunc(new PolylineIntersector(pts));
            var spineLine = Tuple.Create(onSpine, spineDirection);
            
            var radii = ComputeRadii(ptsDistanceFunc, spineLine, progressProjections);
            EnsureFiniteRadii(radii);

            var pStart = spineLine.Item1 + spineProjections.Min() * spineLine.Item2;
            var pEnd = spineLine.Item1 + spineProjections.Max() * spineLine.Item2;

            return Tuple.Create(radii, pStart, pEnd);
        }

        /// <summary>
        /// Computes a straight spine approximation given two silhouette lines, 
        /// </summary>
        /// <param name="l1pts">Polyline defining the first silhouette line.</param>
        /// <param name="l2pts">Polyline defining the second silhouette line.</param>
        /// <param name="progress">The progress points along the spine for which to compute the radii.</param>
        /// <param name="onSpine">A point on the spine</param>
        /// <param name="spineDirection">The direction vector of the spine</param>
        /// <returns>
        /// A tuple (radii, start, end). <c>radii</c> is an array of the radii along the spine points defined by <paramref name="progress"/>. 
        /// <c>start</c> is the spine's starting point and <c>end</c> is the spine's ending point.
        /// </returns>
        public static Tuple<double[], Point, Point> Compute(Point[] l1pts, Point[] l2pts, double[] progress, Point onSpine, Vector spineDirection)
        {
            Contract.Requires(l1pts != null && l1pts.Length >= 2);
            Contract.Requires(l2pts != null && l2pts.Length >= 2);
            Contract.Requires(progress != null && progress.Length >= 2); // we need at-least two spine pints
            Contract.Requires(progress.First() == 0 && progress.Last() == 1);

            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>() != null);
            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>().Item1 != null);
            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>().Item1.Length == progress.Length);

            var spineProjections =
                from pnt in l1pts.Concat(l2pts)
                let t = pnt.ProjectOnLine(onSpine, spineDirection)
                select t;
            spineProjections = spineProjections.ToArray();
            var progressProjections = ComputeProgressProjections(progress, spineProjections);

            var leftDistanceFunc = DistanceFunc(new PolylineIntersector(l1pts));
            var rightDistanceFunc = DistanceFunc(new PolylineIntersector(l2pts));
            var spineLine = Tuple.Create(onSpine, spineDirection);
            var l = ComputeRadii(leftDistanceFunc, spineLine, progressProjections);
            var r = ComputeRadii(rightDistanceFunc, spineLine, progressProjections);

            var radii = ComputeFinalRadii(l, r);
            var pStart = spineLine.Item1 + spineProjections.Min() * spineLine.Item2;
            var pEnd = spineLine.Item1 + spineProjections.Max() * spineLine.Item2;

            return Tuple.Create(radii, pStart, pEnd);
        }
        
        /// <summary>
        /// Computes a straight spine approximation given two silhouette lines and a prior vector describing an approximate direction of the spine.
        /// </summary>
        /// <param name="l1pts">Polyline defining the first silhouette line.</param>
        /// <param name="l2pts">Polyline defining the second silhouette line.</param>
        /// <param name="progress">The progress points along the spine for which to compute the radii.</param>
        /// <param name="prior">The approximate prior vector</param>
        /// <returns>
        /// A tuple (radii, start, end). <c>radii</c> is an array of the radii along the spine points defined by <paramref name="progress"/>. 
        /// <c>start</c> is the spine's starting point and <c>end</c> is the spine's ending point.
        /// </returns>
        public static Tuple<double[], Point, Point> Compute(Point[] l1pts, Point[] l2pts, double[] progress, Vector prior)
        {
            Contract.Requires(l1pts != null && l1pts.Length >= 2);
            Contract.Requires(l2pts != null && l2pts.Length >= 2);
            Contract.Requires(progress != null && progress.Length >= 2); // we need at-least two spine pints
            Contract.Requires(progress.First() == 0 && progress.Last() == 1);

            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>() != null);
            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>().Item1 != null);
            Contract.Ensures(Contract.Result<Tuple<double[], Point, Point>>().Item1.Length == progress.Length);

            var pca2d = PointsPCA2D.Compute(l1pts.Concat(l2pts));
            var lineVec = Math.Abs(pca2d.First * prior) > Math.Abs(pca2d.Second * prior) ? pca2d.First : pca2d.Second;
            var linePoint = pca2d.Mean;

            IEnumerable<double> spineProjections = null;
            IEnumerable<double> progressProjections = null;
            Tuple<Point, Vector> spineLine = null;
            double[] l = null;
            double[] r = null;

            const double ALPHA = 0.05;
            var leftDistanceFunc = DistanceFunc(new PolylineIntersector(l1pts));
            var rightDistanceFunc = DistanceFunc(new PolylineIntersector(l2pts));
            for (int i = 0; i < 100; ++i)
            {
                spineProjections =
                    from pnt in l1pts.Concat(l2pts)
                    let t = pnt.ProjectOnLine(linePoint, lineVec)
                    select t;
                spineProjections = spineProjections.ToArray();

                progressProjections = ComputeProgressProjections(progress, spineProjections);
                spineLine = Tuple.Create(linePoint, lineVec);

                l = ComputeRadii(leftDistanceFunc, spineLine, progressProjections);
                r = ComputeRadii(rightDistanceFunc, spineLine, progressProjections);
                var lGrad = RadiiGradientsApprox(leftDistanceFunc, spineLine, progressProjections);
                var rGrad = RadiiGradientsApprox(rightDistanceFunc, spineLine, progressProjections);

                var allArrays = new double[][] { l, r, lGrad.Item1, lGrad.Item2, lGrad.Item3, lGrad.Item4, rGrad.Item1, rGrad.Item2, rGrad.Item3, rGrad.Item4 };
                var finiteIndices =
                    from k in Enumerable.Range(0, progressProjections.Count())
                    where Enumerable.Range(0, allArrays.Length).All(j => !double.IsInfinity(allArrays[j][k]))
                    select k;

                var dpx = finiteIndices.Select(k => (l[k] - r[k]) * (lGrad.Item1[k] - rGrad.Item1[k])).Average();
                var dpy = finiteIndices.Select(k => (l[k] - r[k]) * (lGrad.Item2[k] - rGrad.Item2[k])).Average();
                var dvx = finiteIndices.Select(k => (l[k] - r[k]) * (lGrad.Item3[k] - rGrad.Item3[k])).Average();
                var dvy = finiteIndices.Select(k => (l[k] - r[k]) * (lGrad.Item4[k] - rGrad.Item4[k])).Average();

                linePoint.X -= ALPHA * dpx;
                linePoint.Y -= ALPHA * dpy;
                lineVec.X -= ALPHA * dvx;
                lineVec.Y -= ALPHA * dvy;
                lineVec.Normalize();
            }

            var radii = ComputeFinalRadii(l, r);

            var pStart = spineLine.Item1 + spineProjections.Min() * spineLine.Item2;
            var pEnd = spineLine.Item1 + spineProjections.Max() * spineLine.Item2;

            return Tuple.Create(radii, pStart, pEnd);
        }

        private static double[] ComputeFinalRadii(double[] leftRadii, double[] rightRadii)
        {
            Contract.Requires(leftRadii != null && rightRadii != null);
            Contract.Requires(leftRadii.Length == rightRadii.Length);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == leftRadii.Length);

            var n = leftRadii.Length;
            var result = new double[n];
            for (int i = 0; i < n; ++i)
            {
                var l = leftRadii[i];
                var r = rightRadii[i];

                if (double.IsInfinity(l) || double.IsInfinity(r))
                    result[i] = double.PositiveInfinity;
                else
                    result[i] = (l + r) / 2;
            }

            EnsureFiniteRadii(result);
            return result;
        }

        private static void EnsureFiniteRadii(double[] result)
        {
            var containsFiniteRadius = result.Any(r => !double.IsInfinity(r));

            if (containsFiniteRadius)
            {
                var n = result.Length;
                var firstFiniteIdx = Enumerable.Range(0, n).First(i => !double.IsInfinity(result[i]));
                var lastFiniteIdx = Enumerable.Range(0, n).Last(i => !double.IsInfinity(result[i]));

                for (int i = 0; i < firstFiniteIdx; ++i)
                    result[i] = result[firstFiniteIdx];
                for (int i = lastFiniteIdx + 1; i < n; ++i)
                    result[i] = result[lastFiniteIdx];
            }
            else
            {
                // we make all radii be a small value (0.01) if no finite radius has been found
                for (int i = 0; i < result.Length; i++)
                    result[i] = 0.01;
            }
        }

        private static IEnumerable<double> ComputeProgressProjections(double[] progress, IEnumerable<double> spineProjections)
        {
            var spineProjMin = spineProjections.Min();
            var spineProjMax = spineProjections.Max();
            var progressProjections =
                from t in progress
                select spineProjMin + t * (spineProjMax - spineProjMin);
            progressProjections.ToArray();
            return progressProjections;
        }

        private static Tuple<double[], double[], double[], double[]> RadiiGradientsApprox(Func<Point, Vector, double> distanceFunc, Tuple<Point, Vector> line, IEnumerable<double> spineProjections, double delta = 1E-3)
        {
            var radii = ComputeRadii(distanceFunc, line, spineProjections);
            
            var dpx = Tuple.Create(new Point(line.Item1.X + delta, line.Item1.Y), line.Item2);
            var dpxRadii = ComputeRadii(distanceFunc, dpx, spineProjections);

            var dpy = Tuple.Create(new Point(line.Item1.X, line.Item1.Y + delta), line.Item2);
            var dpyRadii = ComputeRadii(distanceFunc, dpy, spineProjections);

            var dvx = Tuple.Create(line.Item1, new Vector(line.Item2.X + delta, line.Item2.Y));
            var dvxRadii = ComputeRadii(distanceFunc, dvx, spineProjections);

            var dvy = Tuple.Create(line.Item1, new Vector(line.Item2.X, line.Item2.Y + delta));
            var dvyRadii = ComputeRadii(distanceFunc, dvy, spineProjections);

            for (int i = 0; i < radii.Length; ++i)
            {
                dpxRadii[i] = (dpxRadii[i] - radii[i]) / delta;
                dpyRadii[i] = (dpyRadii[i] - radii[i]) / delta;
                dvxRadii[i] = (dvxRadii[i] - radii[i]) / delta;
                dvyRadii[i] = (dvyRadii[i] - radii[i]) / delta;
            }

            return Tuple.Create(dpxRadii, dpyRadii, dvxRadii, dvyRadii);
        }

        private static double[] ComputeRadii(Func<Point, Vector, double> distanceFunc, Tuple<Point, Vector> line, IEnumerable<double> spineProjections)
        {
            var linePnt = line.Item1;
            var lineDir = line.Item2;
            var lineNormal = new Vector(-lineDir.Y, lineDir.X).Normalized();

            var radii =
                from t in spineProjections
                let pnt = line.Item1 + t * line.Item2
                let distance = distanceFunc(pnt, lineNormal)
                select distance;

            return radii.ToArray();
        }

        private static Func<Point, Vector, double> DistanceFunc(PolylineIntersector intersector)
        {
            Func<Point, Vector, double> result = (pnt, normal) =>
                {
                    var intersection = intersector.IntersectLine(pnt, normal);
                    return intersection != null ? intersection.Item2 : double.PositiveInfinity;
                };
            return result;
        }
    }
}
