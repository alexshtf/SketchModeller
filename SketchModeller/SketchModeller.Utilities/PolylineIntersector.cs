using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// Computes intersection of lines with a given polyline
    /// </summary>
    public class PolylineIntersector
    {
        private readonly Point[] polyline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolylineIntersector"/> class.
        /// </summary>
        /// <param name="polyline">The polyline.</param>
        public PolylineIntersector(IEnumerable<Point> polyline)
        {
            Contract.Requires(polyline != null);

            this.polyline = polyline.ToArray();
        }

        /// <summary>
        /// Computes the intersection of a line with the polyline wrapped by this class.
        /// </summary>
        /// <param name="x">A point on the line</param>
        /// <param name="v">The direction vector of the line</param>
        /// <returns><c>null</c> if there is no intersection, or a tuple containing
        /// the intersection point and its distance from <paramref name="x"/>.</returns>
        public Tuple<Point, double> IntersectLine(Point x, Vector v)
        {
            // ensure distance is non-negative if the intersection is defined (result is not null)
            Contract.Ensures(Contract.Result<Tuple<Point, double>>() == null ||
                Contract.Result<Tuple<Point, double>>().Item2 >= 0);

            Point candidatePoint = default(Point);
            double candidateDistance = double.PositiveInfinity;
            for (int i = 0; i < polyline.Length - 1; ++i)
            {
                Point result;
                double distance;
                if (IntersectSegment(
                    ref polyline[i], ref polyline[i + 1],
                    ref x, ref v,
                    out result, out distance))
                {
                    if (distance < candidateDistance)
                    {
                        candidateDistance = distance;
                        candidatePoint = result;
                    }
                }
            }

            if (double.IsPositiveInfinity(candidateDistance))
                return null;
            else
                return Tuple.Create(candidatePoint, candidateDistance);
        }

        /// <summary>
        /// Computes intersection of a segment with a line
        /// </summary>
        /// <param name="p">First segment point</param>
        /// <param name="q">Second segment point</param>
        /// <param name="x">A point on the line</param>
        /// <param name="v">The direction vector of the line</param>
        /// <param name="result">The resulting intersection point</param>
        /// <param name="distance">The resulting intersection distance</param>
        /// <returns><c>true</c> if and only if the intersection exists</returns>
        /// <remarks>Algorithm from http://paulbourke.net/geometry/lineline2d/ </remarks>
        private static bool IntersectSegment(
            ref Point p, ref Point q,
            ref Point x, ref Vector v,
            out Point result, out double distance)
        {
            var x1 = p.X;
            var x2 = q.X;
            var x3 = x.X;
            var x4 = x.X + v.X;

            var y1 = p.Y;
            var y2 = q.Y;
            var y3 = x.Y;
            var y4 = x.Y + v.Y;

            var numerator = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
            var denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            var ua = numerator / denominator;

            if (double.IsNaN(ua) || ua > 1 || ua < 0)
            {
                result = default(Point);
                distance = double.PositiveInfinity;
                return false;
            }

            result = WpfUtils.Lerp(p, q, ua);
            distance = (x - result).Length;
            return true;
        }
    }
}
