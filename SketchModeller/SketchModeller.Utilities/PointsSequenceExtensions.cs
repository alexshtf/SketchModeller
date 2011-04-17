using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// Extension methods for the <see cref="PointsSequence"/> class
    /// </summary>
    public static class PointsSequenceExtensions
    {
        public static double ComputeLength(this PointsSequence seq)
        {
            Contract.Requires(seq != null);
            Contract.Ensures(Contract.Result<double>() >= 0);

            var points = (IEnumerable<Point>)seq.Points;
            if (seq is Polygon)
                points = points.Append(points.First());

            return ComputeCurveLength(points);
        }

        public static double ComputeCurveLength(this IEnumerable<Point> points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Any() && points.Skip(1).Any());
            Contract.Ensures(Contract.Result<double>() >= 0);

            var segmentLengths =
                from pair in points.SeqPairs()
                select (pair.Item1 - pair.Item2).Length;

            return segmentLengths.Sum();
        }
    }
}
