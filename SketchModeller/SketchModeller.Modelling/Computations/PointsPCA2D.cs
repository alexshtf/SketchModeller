using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;
using Meta.Numerics.Statistics;

namespace SketchModeller.Modelling.Computations
{
    public class PCA2DResult
    {
        public readonly Point Mean;
        public readonly Vector First;
        public readonly Vector Second;

        public PCA2DResult(Point mean, Vector first, Vector second)
        {
            Mean = mean;
            First = first;
            Second = second;
        }
    }

    public static class PointsPCA2D
    {
        public static PCA2DResult Compute(IEnumerable<Point> points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Any() && points.Skip(1).Any()); // at least two points

            var xAvg = points.Select(pnt => pnt.X).Average();
            var yAvg = points.Select(pnt => pnt.Y).Average();

            var shiftedPoints =
                from pnt in points
                select new Point(pnt.X - xAvg, pnt.Y - yAvg);

            var mvSample = new MultivariateSample(2);
            foreach (var pnt in shiftedPoints)
                mvSample.Add(pnt.X, pnt.Y);

            var pca = mvSample.PrincipalComponentAnalysis();
            var first = pca.Component(0).NormalizedVector();
            var second = pca.Component(1).NormalizedVector();

            return new PCA2DResult(
                new Point(xAvg, yAvg),
                new Vector(first[0], first[1]),
                new Vector(second[0], second[1]));
        }
    }
}
