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
        /// <summary>
        /// Calculates linear interpolation between two points (1 - t) * p1 + t * p2
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="t">Interpolation factor</param>
        /// <returns>The resulting interpolated point</returns>
        public static Point Lerp(Point p1, Point p2, double t)
        {
            var vec = t * (p2 - p1);
            return p1 + vec;
        }

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
        public static Tuple<Point, double, int> ProjectionOnCurve(this Point point, IEnumerable<Point> curve)
        {
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

        public static Point ProjectOnSegment(this Point pnt, Point segStart, Point segEnd)
        {
            var u = pnt - segStart;
            var v = segEnd - segStart;
            var t = (u * v) / (v * v);
            if (t < 0)           // to the "left" of segStart
                return segStart;
            else if (t > 1)      // to the "right" of segEnd
                return segEnd;
            else                 // between segStart and segEnd
                return segStart + t * v;
        }

        public static double DistanceFromSegment(this Point pnt, Point segStart, Point segEnd)
        {
            return (pnt - pnt.ProjectOnSegment(segStart, segEnd)).Length;
        }

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

        public static double SquareDiff(IEnumerable<Point> first, IEnumerable<Point> second)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(first.Count() == second.Count());
            Contract.Ensures(Contract.Result<double>() >= 0.0);

            return first.Zip(second).Sum(pair => (pair.Item1 - pair.Item2).LengthSquared);
        }

        public static Vector Perp(this Vector v)
        {
            return new Vector(v.Y, -v.Y);
        }

        public static Vector Normalized(this Vector v)
        {
            v.Normalize();
            return v;
        }
    }
}
