using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;

namespace SketchToCyl
{
    static class CurveFilter
    {
        public static Point[] IterativeAverageFilter(IList<Point> points, double changeThreshold)
        {
            var prevDiff = double.MaxValue;
            Point[] current = points.ToArray();
            Point[] prev = null;
            while (prevDiff >= changeThreshold)
            {
                prev = current;
                current = AverageFilter(current);
                prevDiff = WpfUtils.SquareDiff(prev, current);
            }

            return current;
        }

        public static Point[] AverageFilter(IList<Point> points)
        {
            IEnumerable<Point> averages = null;
            if (points.Count > 2)
            {
                averages = from triple in points.SeqTripples()
                           let v1 = (Vector)triple.Item1
                           let v2 = (Vector)triple.Item2
                           let v3 = (Vector)triple.Item3
                           let avg = 0.25 * v1 + 0.5 * v2 + 0.25 * v3
                           select (Point)avg;
            }
            else
                averages = System.Linq.Enumerable.Empty<Point>();

            return
                Utils.Enumerable.Singleton(points.First())
                .Concat(averages)
                .Concat(Utils.Enumerable.Singleton(points.Last()))
                .ToArray();
        }
    }
}
