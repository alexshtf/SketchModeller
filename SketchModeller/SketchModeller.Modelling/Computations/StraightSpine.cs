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
        public static Tuple<double[], Point, Point> Compute(Point[] l1pts, Point[] l2pts, double[] progress, Vector prior)
        {
            var pca2d = PointsPCA2D.Compute(l1pts.Concat(l2pts));
            var lineVec = Math.Abs(pca2d.First * prior) > Math.Abs(pca2d.Second * prior) ? pca2d.First : pca2d.Second;
            var linePoint = pca2d.Mean;

            IEnumerable<double> spineProjections = null;
            IEnumerable<double> progressProjections = null;
            Tuple<Point, Vector> spineLine = null;
            double[] l = null;
            double[] r = null;

            const double ALPHA = 0.05;
            for (int i = 0; i < 100; ++i)
            {
                spineProjections =
                    from pnt in l1pts.Concat(l2pts)
                    let t = pnt.ProjectOnLine(linePoint, lineVec)
                    select t;
                spineProjections = spineProjections.ToArray();

                progressProjections = ComputeProgressProjections(progress, spineProjections);
                spineLine = Tuple.Create(linePoint, lineVec);

                l = ComputeRadii(l1pts, spineLine, progressProjections);
                r = ComputeRadii(l2pts, spineLine, progressProjections);
                var lGrad = RadiiGradientsApprox(l1pts, spineLine, progressProjections);
                var rGrad = RadiiGradientsApprox(l2pts, spineLine, progressProjections);

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

            var radii = ComputeRadiiFromLeftRight(l, r);

            var pStart = spineLine.Item1 + spineProjections.Min() * spineLine.Item2;
            var pEnd = spineLine.Item1 + spineProjections.Max() * spineLine.Item2;

            return Tuple.Create(radii, pStart, pEnd);
        }

        private static double[] ComputeRadiiFromLeftRight(double[] leftRadii, double[] rightRadii)
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

            var firstFiniteIdx = Enumerable.Range(0, n).First(i => !double.IsInfinity(result[i]));
            var lastFiniteIdx = Enumerable.Range(0, n).Last(i => !double.IsInfinity(result[i]));

            for (int i = 0; i < firstFiniteIdx; ++i)
                result[i] = result[firstFiniteIdx];
            for (int i = lastFiniteIdx + 1; i < n; ++i)
                result[i] = result[lastFiniteIdx];

            return result;
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

        private static Tuple<double[], double[], double[], double[]> RadiiGradientsApprox(Point[] polyline, Tuple<Point, Vector> line, IEnumerable<double> spineProjections, double delta = 1E-3)
        {
            var radii = ComputeRadii(polyline, line, spineProjections);
            
            var dpx = Tuple.Create(new Point(line.Item1.X + delta, line.Item1.Y), line.Item2);
            var dpxRadii = ComputeRadii(polyline, dpx, spineProjections);

            var dpy = Tuple.Create(new Point(line.Item1.X, line.Item1.Y + delta), line.Item2);
            var dpyRadii = ComputeRadii(polyline, dpy, spineProjections);

            var dvx = Tuple.Create(line.Item1, new Vector(line.Item2.X + delta, line.Item2.Y));
            var dvxRadii = ComputeRadii(polyline, dvx, spineProjections);

            var dvy = Tuple.Create(line.Item1, new Vector(line.Item2.X, line.Item2.Y + delta));
            var dvyRadii = ComputeRadii(polyline, dvy, spineProjections);

            for (int i = 0; i < radii.Length; ++i)
            {
                dpxRadii[i] = (dpxRadii[i] - radii[i]) / delta;
                dpyRadii[i] = (dpyRadii[i] - radii[i]) / delta;
                dvxRadii[i] = (dvxRadii[i] - radii[i]) / delta;
                dvyRadii[i] = (dvyRadii[i] - radii[i]) / delta;
            }

            return Tuple.Create(dpxRadii, dpyRadii, dvxRadii, dvyRadii);
        }

        private static double[] ComputeRadii(Point[] polyline, Tuple<Point, Vector> line, IEnumerable<double> spineProjections)
        {
            var linePnt = line.Item1;
            var lineDir = line.Item2;
            var lineNormal = new Vector(-lineDir.Y, lineDir.X).Normalized();

            var intersector = new PolylineIntersector(polyline);

            var radii =
                from t in spineProjections
                let pnt = line.Item1 + t * line.Item2
                let intersection = intersector.IntersectLine(pnt, lineNormal)
                let distance = intersection != null ? intersection.Item2 : double.PositiveInfinity
                select distance;

            return radii.ToArray();
        }

        private static double[] ComputeRadii(
           Point[] l1pts, Point[] l2pts,
           Tuple<Point, Vector> spineLine,
           IEnumerable<double> spineProjections)
        {
            var spineLineNormal = new Vector(-spineLine.Item2.Y, spineLine.Item2.X).Normalized();
            var straightSpinePoints =
                (from t in spineProjections
                 select spineLine.Item1 + t * spineLine.Item2).ToArray();
            var straightSpineNormals =
                Enumerable.Repeat(spineLineNormal, straightSpinePoints.Length)
                .ToArray();

            var radii = CurvedSubdividor.ComputeRadii(
                straightSpinePoints,
                straightSpineNormals,
                l1pts,
                l2pts);
            return radii;
        }
    }
}
