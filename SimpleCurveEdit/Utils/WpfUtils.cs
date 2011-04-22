using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class WpfUtils
    {
        public static DependencyObject HitTestFirst(this Visual reference, HitTestParameters htParams, Predicate<DependencyObject> filter)
        {
            Contract.Requires(reference != null);
            Contract.Requires(htParams != null);
            Contract.Requires(filter != null);
            Contract.Ensures(Contract.Result<DependencyObject>() == null || filter(Contract.Result<DependencyObject>()));

            DependencyObject result = null;
            VisualTreeHelper.HitTest(
                reference,
                null,
                htResult =>
                {
                    var visual = htResult.VisualHit;
                    if (filter(visual))
                    {
                        result = visual;
                        return HitTestResultBehavior.Stop;
                    }
                    else
                        return HitTestResultBehavior.Continue;
                },
                htParams);
            return result;
        }

        public static IList<DependencyObject> HitTestAll(this Visual reference, HitTestParameters htParams, Predicate<DependencyObject> filter = null)
        {
            Contract.Requires(reference != null);
            Contract.Requires(htParams != null);
            Contract.Ensures(Contract.Result<IList<DependencyObject>>() != null);
            Contract.Ensures(filter == null || Contract.ForAll(Contract.Result<IList<DependencyObject>>(), d => filter(d)));

            // null filter means filter nothing (we return everything we can)
            if (filter == null)
                filter = _ => true;

            var result = new List<DependencyObject>();
            VisualTreeHelper.HitTest(
                reference,
                null,
                htResult =>
                {
                    var visual = htResult.VisualHit;
                    if (filter(visual))
                        result.Add(visual);
                    return HitTestResultBehavior.Continue;
                },
                htParams);
            return result;
        }

        public static Vector IgnoreZ(this Vector3D p)
        {
            return new Vector(p.X, p.Y);
        }

        /// <summary>
        /// Checks wether the specified point has finite coordinates.
        /// </summary>
        /// <param name="p">The point to be checked.</param>
        /// <returns><c>true</c> if and only if the point <paramref name="p"/>'s coordinates are finite values.</returns>
        [Pure]
        public static bool IsFinite(this Point3D p)
        {
            return
                !double.IsNaN(p.X) &&
                !double.IsNaN(p.Y) &&
                !double.IsNaN(p.Z) &&
                !double.IsInfinity(p.X) &&
                !double.IsInfinity(p.Y) &&
                !double.IsInfinity(p.Z);
        }

        /// <summary>
        /// Calculates linear interpolation between two points (1 - t) * p1 + t * p2
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="t">Interpolation factor</param>
        /// <returns>The resulting interpolated point</returns>
        [Pure]
        public static Point Lerp(Point p1, Point p2, double t)
        {
            var vec = t * (p2 - p1);
            return p1 + vec;
        }

        [Pure]
        public static Point ToPoint(this string[] values)
        {
            Contract.Requires(values.Length >= 2);
            var x = double.Parse(values[0]);
            var y = double.Parse(values[1]);

            return new Point(x, y);
        }

        /// <summary>
        /// Projects a 2D point on a 2D curve
        /// </summary>
        /// <param name="point">The point to be projected</param>
        /// <param name="curve">The curve to project on</param>
        /// <returns>A tripple containing the projection result, the minimal distance from <paramref name="point"/> to the curve,
        /// and the index of the segment in <paramref name="curve"/> that minimizes this distance</returns>
        /// <remarks>
        /// In case <paramref name="curve"/> is a zero-length array the method returns a triple with distance of <c>Double.NaN</c>
        /// and segment index <c>-1</c>. When <paramref name="curve"/> is an array with a single point, the method returns this point
        /// as the projection result, the distance between <c>curve[0]</c> and <c>point</c> as the minimizing distance and <c>-1</c>
        /// as the segment index (we have no segments, so we cannot return a valid segment index). Otherwise, the method performs
        /// as expected.
        /// </remarks>
        [Pure]
        public static Tuple<Point, double, int> ProjectionOnCurve(this Point point, IEnumerable<Point> curve)
        {
            Contract.Requires(curve != null);
            Contract.Requires(curve.Count() >= 2);

            Contract.Ensures(Contract.Result<Tuple<Point, double, int>>().Item2 >= 0); // distance is greater than zero
            Contract.Ensures(Contract.Result<Tuple<Point, double, int>>().Item3 >= 0); // index is greater than zero
            Contract.Ensures(Contract.Result<Tuple<Point, double, int>>().Item3 < curve.Count() - 1); // segment index is less than num of segments
            // The projected point is closer to "point" than any other point on the curve.
            Contract.Ensures(Contract.ForAll(curve, curvePoint => 
                (curvePoint - point).Length >= Contract.Result<Tuple<Point, double, int>>().Item2));

            var count = curve.Count();

            var projectedPoints =
                from indexedSegment in curve.SeqPairs().ZipIndex()
                let index = indexedSegment.Index
                let segment = indexedSegment.Value
                let segStart = segment.Item1
                let segEnd = segment.Item2
                let projectedPoint = point.ProjectOnSegment(segStart, segEnd)
                let distance = (point - projectedPoint).Length
                select Tuple.Create(projectedPoint, distance, index);

            return projectedPoints.Minimizer(pair => pair.Item2); // Item2 is the distance
        }

        /// <summary>
        /// Computes the distance from the given point to the given line.
        /// </summary>
        /// <param name="pnt">The point to compute the distance for</param>
        /// <param name="p">First point defining the line</param>
        /// <param name="q">Second point defining the line</param>
        /// <returns>The distance from <paramref name="pnt"/> to the line defined by <paramref name="p"/> and <paramref name="q"/>.</returns>
        public static double DistanceToLine(this Point pnt, Point p, Point q)
        {
            return (pnt - pnt.ProjectOnLine(p, q)).Length;
        }

        /// <summary>
        /// Projects a point on a line passing through p and q
        /// </summary>
        /// <param name="pnt">The point to be projected</param>
        /// <param name="p">First point defining the line</param>
        /// <param name="q">Second point defining the line</param>
        /// <returns>The point <paramref name="pnt"/> projected on the specified line segment.</returns>
        public static Point ProjectOnLine(this Point pnt, Point p, Point q)
        {
            var v = q - p;
            var u = pnt - p;

            var t = (u * v) / v.LengthSquared;
            var candidate = p + t * v;

            return candidate;
        }

        /// <summary>
        /// Projects a point on a segment.
        /// </summary>
        /// <param name="pnt">The point to project</param>
        /// <param name="segStart">Starting segment point</param>
        /// <param name="segEnd">Ending segment point</param>
        /// <returns>The point on the segment that minimizes the distance to pnt.</returns>
        [Pure]
        public static Point ProjectOnSegment(this Point pnt, Point segStart, Point segEnd)
        {
            Contract.Ensures((pnt - Contract.Result<Point>()).LengthSquared <= (pnt - segStart).LengthSquared);
            Contract.Ensures((pnt - Contract.Result<Point>()).LengthSquared <= (pnt - segEnd).LengthSquared);

            var v = segEnd - segStart;
            if (v.LengthSquared <= double.Epsilon) // segment is of length almost zero. Therefore any point is valid.
                return segStart;

            var u = pnt - segStart;
            var t = (u * v) / (v * v);

            if (t < 0)           // to the "left" of segStart
                return segStart;
            else if (t > 1)      // to the "right" of segEnd
                return segEnd;
            else                 // between segStart and segEnd
            {
                // the point segStart + t * v is still considered a candidate because of a numerical error that can occur
                // in the computation of "t". So we still need to choose the point with minimal distance to "pnt".
                // We do it to ensure the contract above is correct - that is, we find the closest point to "pnt" on the segment.
                var candidate = segStart + t * v;

                var potentialResults = new Tuple<Point, double>[]
                {
                    Tuple.Create(candidate, (candidate - pnt).LengthSquared),
                    Tuple.Create(segStart, (segStart - pnt).LengthSquared),
                    Tuple.Create(segEnd, (segEnd - pnt).LengthSquared),
                };

                return potentialResults.Minimizer(x => x.Item2).Item1; 
            }
        }

        /// <summary>
        /// Calculates the minimum distance from a point to any point on a line segment.
        /// </summary>
        /// <param name="pnt">The point to calculate distance from.</param>
        /// <param name="segStart">The starting segment point</param>
        /// <param name="segEnd">Ending segment point.</param>
        /// <returns>The minimum distance between <paramref name="point"/> and any point on the segment between
        /// <paramref name="segStart"/> and <paramref name="segEnd"/>.</returns>
        [Pure]
        public static double DistanceFromSegment(this Point pnt, Point segStart, Point segEnd)
        {
            Contract.Ensures(Contract.Result<double>() >= 0);

            return (pnt - pnt.ProjectOnSegment(segStart, segEnd)).Length;
        }

        /// <summary>
        /// Gets the viewport that the specified <see cref="Visual3D"/> object is displayed in.
        /// </summary>
        /// <param name="visual3d"></param>
        /// <returns></returns>
        public static Viewport3D GetViewport(this Visual3D visual3d)
        {
            var path = VisualPathUp(visual3d);
            return path.OfType<Viewport3D>().FirstOrDefault();
        }

        public static IEnumerable<object> VisualPathUp(this DependencyObject reference)
        {
            while (reference != null)
            {
                yield return reference;
                reference = VisualTreeHelper.GetParent(reference);
            }
        }

        public static IEnumerable<Point3D> Transform(this IEnumerable<Point3D> source, Transform3D transform)
        {
            return from pnt in source
                   select transform.Transform(pnt);
        }

        public static IEnumerable<object> VisualTree(this DependencyObject parent)
        {
            yield return parent;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foreach(var item in child.VisualTree())
                    yield return item;
            }
        }

        [Pure]
        public static double SquareDiff(IEnumerable<Point> first, IEnumerable<Point> second)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(first.Count() == second.Count());
            Contract.Ensures(Contract.Result<double>() >= 0.0);

            return first.Zip(second).Sum(pair => (pair.Item1 - pair.Item2).LengthSquared);
        }

        [Pure]
        public static Vector Perp(this Vector v)
        {
            return new Vector(v.Y, -v.Y);
        }

        [Pure]
        public static Vector Normalized(this Vector v)
        {
            v.Normalize();
            return v;
        }
    }
}
