using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows;
using SketchModeller.Modelling.Services.Snap;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Computations
{
    class SubdivisionResult
    {
        public Point[] SpinePoints { get; set; }
        public Vector[] Normals { get; set; }
    }

    static class CurvedSubdividor
    {
        public static SubdivisionResult Subdivide(Point[] l1, Point[] l2)
        {
            // create a polygon from l1, l2: we assume here that l1 and l2 have the same direction
            var polygon = l1.Concat(l2.Reverse()).ToArray();

            var filteredPoints = TwoLinesMedialAxis.Compute(l1, l2, polygon, proximityDistance: 3.0);

            // connect the extreme points with a long path (dijkstra algorithm)
            var path = PointsToPolylineConverter.Convert(filteredPoints);

            // smooth the path
            var smoothed = SmoothPath(path);

            var points = smoothed.Item1;
            var normals = AverageSmoothNormals(smoothed.Item2);

            // create the result
            return new SubdivisionResult
            {
                SpinePoints = points,
                Normals = normals,
            };
        }

        public static double[] ComputeRadii(Point[] points, Vector[] normals, Point[] l1, Point[] l2)
        {
            Contract.Requires(points != null);
            Contract.Requires(normals != null);
            Contract.Requires(points.Length == normals.Length);
            Contract.Requires(l1 != null);
            Contract.Requires(l2 != null);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == points.Length);

            var intersector1 = new PolylineIntersector(l1);
            var intersector2 = new PolylineIntersector(l2);
            var radii = ComputeRadii(
                points,
                normals,
                (p, n) => IfNull(intersector1.IntersectLine(p, n), t => t.Item2, double.PositiveInfinity),
                (p, n) => IfNull(intersector2.IntersectLine(p, n), t => t.Item2, double.PositiveInfinity));

            radii = AverageSmoothRadii(radii);

            return radii;
        }

        private static double[] ComputeRadii(
            Point[] points,
            Vector[] normals,
            Func<Point, Vector, double> l1,
            Func<Point, Vector, double> l2)
        {
            Contract.Requires(points != null);
            Contract.Requires(normals != null);
            Contract.Requires(points.Length == normals.Length);
            Contract.Requires(l1 != null);
            Contract.Requires(l2 != null);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == points.Length);

            var n = points.Length;
            var resultQuery =
                from i in Enumerable.Range(0, n)
                let r1 = l1(points[i], normals[i])
                let r2 = l2(points[i], normals[i])
                select Math.Min(r1, r2);

            return resultQuery.ToArray();
        }

        private static Tuple<Point[], Vector[]> SmoothPath(Point[] path)
        {
            path = AverageSmoothMidPoints(path);
            var xs = path.Select(pnt => pnt.X).ToArray();
            var ys = path.Select(pnt => pnt.Y).ToArray();

            var optimalFit = LSPointsIntervalsFitter.FitOptimalIntervals(xs, ys);
            var xInterval = optimalFit.Item1;
            var yInterval = optimalFit.Item2;
            var breakIndices = optimalFit.Item3;

            var smoothedPoints = new Point[path.Length];
            var smoothedNormals = new Vector[path.Length];
            for (int t = 0; t < path.Length; ++t)
            {
                var sx = IntervalsSampler.SampleIntervals(xInterval, t, breakIndices);
                var sy = IntervalsSampler.SampleIntervals(yInterval, t, breakIndices);

                smoothedPoints[t] = new Point(sx.Value, sy.Value);
                smoothedNormals[t] = new Vector(-sy.Derivative, sx.Derivative); // perpendicular to the tangent vector (x, y) => (-y, x)
                smoothedNormals[t].Normalize();
            }

            return Tuple.Create(smoothedPoints, smoothedNormals);
        }

        private static double[] AverageSmoothRadii(double[] radii, double amount = 0.1, int count = 100)
        {
            Func<double[], double[]> smooth = vals => AverageSmooth.SmoothKeepEdges(vals, amount);

            var result = EnumerableExtensions.Generate(radii, smooth)
                .Take(count)
                .Last();

            return result;
        }

        private static Vector[] AverageSmoothNormals(Vector[] normals, double amount = 0.1, int count = 100)
        {
            Func<double[], double[]> scalarSmooth =
                vals => AverageSmooth.Smooth(vals, amount);

            Func<Vector[], Vector[]> singleSmooth =
                vecs => AverageSmooth.SmoothVectors(vecs, scalarSmooth);

            var result =
                EnumerableExtensions.Generate(normals, singleSmooth)
                .Take(count)
                .Last();

            return result;
        }

        private static Point[] AverageSmoothMidPoints(Point[] points, double amount = 0.1, int count = 10)
        {
            Func<double[], double[]> scalarSmooth =
                vals => AverageSmooth.SmoothKeepEdges(vals, amount);

            Func<Point[], Point[]> singleSmooth =
                vecs => AverageSmooth.SmoothPoints(vecs, scalarSmooth);

            var result =
                EnumerableExtensions.Generate(points, singleSmooth)
                .Take(count)
                .Last();

            return result;
        }

        private static T IfNull<T, S>(S source, Func<S, T> func, T valueForNull)
            where S : class
        {
            Contract.Requires(func != null);
            Contract.Ensures(source != null || object.Equals(Contract.Result<T>(), valueForNull));

            if (source == null)
                return valueForNull;
            else
                return func(source);
        }
    }
}
