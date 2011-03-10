using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using Utils;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Approximates a polyline by a single line segment.
    /// </summary>
    public static class SegmentApproximator
    {

        /// <summary>
        /// Approximates a line segment best representing a given polyline.
        /// </summary>
        /// <param name="seq">The points sequence object reprsenting the polyline.</param>
        /// <returns></returns>
        public static Tuple<Point, Point> ApproximateSegment(IList<Point> seq)
        {
            Contract.Requires(seq != null);
            Contract.Requires(seq.Count >= 2);
            Contract.Ensures(Contract.Result<Tuple<Point, Point>>() != null);

            var line = ApproximateLine(seq);
            var p = line.Item1; // point on the line
            var v = line.Item2; // line's direction vector.

            Func<Point, double> t = x => ((x - p) * v) / v.LengthSquared;
            var minPoint = seq.Minimizer(x => t(x));
            var maxPoint = seq.Minimizer(x => -t(x));

            return Tuple.Create(minPoint, maxPoint);
        }

        /// <summary>
        /// Approximates a line passing near a polyline using PCA.
        /// </summary>
        /// <param name="seq">The points sequence object representing the polyline</param>
        /// <returns>A tuple containing a point on the line and its direction vector</returns>
        public static Tuple<Point, Vector> ApproximateLine(IList<Point> seq)
        {
            Contract.Requires(seq != null);
            Contract.Requires(seq.Count >= 2);
            Contract.Ensures(Contract.Result<Tuple<Point, Vector>>() != null);

            var pca = seq.PCA();

            var centroid = pca.Item1;
            var mainAxis = pca.Item2;

            return Tuple.Create(centroid, mainAxis);
        }
    }
}
