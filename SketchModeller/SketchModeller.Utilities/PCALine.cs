using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Meta.Numerics.Statistics;
using System.Diagnostics.Contracts;
using Utils;

namespace SketchModeller.Utilities
{
    static class PCALine
    {
        public static Tuple<Point, Vector> Compute(Point[] points)
        {
            Contract.Requires(points.Length >= 2);
            Contract.Ensures(Contract.Result<Tuple<Point, Vector>>() != null);

            if (points.Length == 2)
                return Tuple.Create(points[0], (points[1] - points[0]).Normalized());
            
            var avgX = points.Select(p => p.X).Average();
            var avgY = points.Select(p => p.Y).Average();

            var shifted = points.Select(p => p - new Vector(avgX, avgY));
            var mvSample = new MultivariateSample(2);
            foreach (var p in shifted)
                mvSample.Add(p.X, p.Y);

            var pca = mvSample.PrincipalComponentAnalysis();
            var firstComponentVector = pca.Component(0).NormalizedVector();

            return Tuple.Create(
                new Point(avgX, avgY),
                new Vector(firstComponentVector[0], firstComponentVector[1]));
        }
    }
}
