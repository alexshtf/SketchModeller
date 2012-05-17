using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace Utils
{
    /// <summary>
    /// Utilities for 3D Math
    /// </summary>
    public static class MathUtils3D
    {
        private const double EPSILON = 1E-5;

        /// <summary>
        /// The zero 3D vector.
        /// </summary>
        public static readonly Vector3D ZeroVector = new Vector3D();

        /// <summary>
        /// The zero 3D point.
        /// </summary>
        public static readonly Point3D Origin = new Point3D();


        /// <summary>
        /// Unit vector along the X axis.
        /// </summary>
        public static readonly Vector3D UnitX = new Vector3D(1, 0, 0);

        /// <summary>
        /// Unit vector along the Y axis.
        /// </summary>
        public static readonly Vector3D UnitY = new Vector3D(0, 1, 0);

        /// <summary>
        /// Unit vector along the Z axis.
        /// </summary>
        public static readonly Vector3D UnitZ = new Vector3D(0, 0, 1);

        /// <summary>
        /// Returns a normalized version of the specified vector.
        /// </summary>
        /// <param name="v">The vector to normalize.</param>
        /// <returns>A vector of the same direction as <paramref name="v"/> and magnitude 1.</returns>
        [Pure]
        public static Vector3D Normalized(this Vector3D v)
        {
            v.Normalize();
            return v;
        }

        /// <summary>
        /// Returns the centroid (center of mass) of the specified collection of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The centroid point</returns>
        [Pure]
        public static Point3D Centroid(this IEnumerable<Point3D> points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Any() == true);

            var vectors = from pnt in points
                          select pnt - Origin;
            var average = vectors.Aggregate(new Vector3D(), (x, y) => x + y) / (double)vectors.Count();

            return Origin + average;
        }

        /// <summary>
        /// Finds a normalized vector that is most similar, in the inner-product sense, to a specified vector and is orthogonal
        /// to another vector.
        /// </summary>
        /// <param name="similarTo">The vector to which we find the most similar result</param>
        /// <param name="orthogonalTo">The vector to which the result will be orthogonal</param>
        /// <returns>A vector that is orthogonal to <paramref name="orthogonalTo"/>, normalized, and most similar in the 
        /// inner product sense to <paramref name="similarTo"/>.</returns>
        public static Vector3D MostSimilarPerpendicular(Vector3D similarTo, Vector3D orthogonalTo)
        {
            Contract.Requires(!AreParallel(similarTo, orthogonalTo));
            Contract.Ensures(AreOrthogonal(orthogonalTo, Contract.Result<Vector3D>())); // the result is really orthogonal

            orthogonalTo.Normalize();

            var normalizer = 1 / Vector3D.CrossProduct(similarTo, orthogonalTo).Length;
            var dot = Vector3D.DotProduct(similarTo, orthogonalTo);

            return normalizer * new Vector3D(
                similarTo.X - orthogonalTo.X * dot,
                similarTo.Y - orthogonalTo.Y * dot,
                similarTo.Z - orthogonalTo.Z * dot);
        }

        /// <summary>
        /// Tests wether the two given vectors are orthogonal.
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>Returns <c>true</c> if and only if <paramref name="v1"/> is orthogonal to <paramref name="v2"/>.</returns>
        /// <remarks>
        /// Orthogonality cannot be exactly measured because of numeric round-off errors. Two vectors are orthogonal
        /// if their normalized inner product's absolute value is less than some threshold (very close to zero).</remarks>
        [Pure]
        public static bool AreOrthogonal(Vector3D v1, Vector3D v2)
        {
            if (v1.LengthSquared > 0)
                v1.Normalize();

            if (v2.LengthSquared > 0)
                v2.Normalize();

            var dot = Vector3D.DotProduct(v1, v2);

            return Math.Abs(dot) < 3 * NumericUtils.DBL_MACHINE_EPSILON;
        }

        /// <summary>
        /// Tests wether two given vectors are parallel.
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <returns>Returns <c>true</c> if and only if the given vectors are parallel.</returns>
        /// <remarks>
        /// Parallelism cannot be exactly measured due to numeric round-off errors. Therefore two
        /// vectors are parallel if the absolute value of their normalized cross-product is less than some threshold (very close
        /// to zero)</remarks>
        [Pure]
        public static bool AreParallel(Vector3D v1, Vector3D v2)
        {
            Contract.Requires(v1 != ZeroVector);
            Contract.Requires(v2 != ZeroVector);

            v1.Normalize();
            v2.Normalize();
            var cross = Vector3D.CrossProduct(v1, v2);

            return cross.LengthSquared < EPSILON;
        }

        /// <summary>
        /// Calculates linear interpolation between two points (1 - t) * p1 + t * p2
        /// </summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="t">Interpolation factor</param>
        /// <returns>The resulting interpolated point</returns>
        public static Point3D Lerp(Point3D p1, Point3D p2, double t)
        {
            var vec = t * (p2 - p1);
            return p1 + vec;
        }

        /// <summary>
        /// Calculates linear interpolation between two vectors (1 - t) * v1 + t * v2
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <param name="t">Interpolation factor</param>
        /// <returns>The resulting interpolated point</returns>
        public static Vector3D Lerp(Vector3D v1, Vector3D v2, double t)
        {
            return (1 - t) * v1 + t * v2;
        }

        /// <summary>
        /// Projects one vector on another vector (orthogonal projection)
        /// </summary>
        /// <param name="projectWhat">The vector to project</param>
        /// <param name="projectOn">The vector to project on</param>
        /// <returns>The projection of <paramref name="projectWhat"/> on <paramref name="projectOn"/>.</returns>
        public static Vector3D ProjectVector(Vector3D projectWhat, Vector3D projectOn)
        {
            var t = Vector3D.DotProduct(projectWhat, projectOn) / projectOn.LengthSquared;
            return t * projectOn;
        }

        /// <summary>
        /// Projects a point on a line
        /// </summary>
        /// <param name="projectWhat">The point to project</param>
        /// <param name="linePoint">A point on the line to project on</param>
        /// <param name="lineAxis">The axis vector of the line to project on.</param>
        /// <returns>A tuple, such that the first value is the parameter of the projected point on the line's parametric
        /// representation and the second value is the point itself (lazily initialized).</returns>
        public static Tuple<double, Lazy<Point3D>> ProjectOnLine(Point3D projectWhat, Point3D linePoint, Vector3D lineAxis)
        {
            var t = Vector3D.DotProduct(projectWhat - linePoint, lineAxis) / lineAxis.LengthSquared;
            var point = new Lazy<Point3D>(() => linePoint + t * lineAxis);
            return Tuple.Create(t, point);
        }

        /// <summary>
        /// Projects a point on a segment.
        /// </summary>
        /// <param name="pnt">The point to project</param>
        /// <param name="segStart">Starting segment point</param>
        /// <param name="segEnd">Ending segment point</param>
        /// <returns>The point on the segment that minimizes the distance to pnt.</returns>
        [Pure]
        public static Point3D ProjectOnSegment(this Point3D pnt, Point3D segStart, Point3D segEnd)
        {
            Contract.Ensures((pnt - Contract.Result<Point3D>()).LengthSquared <= (pnt - segStart).LengthSquared);
            Contract.Ensures((pnt - Contract.Result<Point3D>()).LengthSquared <= (pnt - segEnd).LengthSquared);

            var v = segEnd - segStart;
            if (v.LengthSquared <= double.Epsilon) // segment is of length almost zero. Therefore any point is valid.
                return segStart;

            var u = pnt - segStart;
            var t = Vector3D.DotProduct(u, v) / Vector3D.DotProduct(v, v);

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

                var potentialResults = new Tuple<Point3D, double>[]
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
        public static double DistanceFromSegment(this Point3D pnt, Point3D segStart, Point3D segEnd)
        {
            Contract.Ensures(Contract.Result<double>() >= 0);

            return (pnt - pnt.ProjectOnSegment(segStart, segEnd)).Length;
        }

        /// <summary>
        /// Projects a 3D point on a 3D curve
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
        public static CurveProjectionResult3D ProjectionOnCurve(this Point3D point, IEnumerable<Point3D> curve)
        {
            Contract.Requires(curve != null);
            Contract.Requires(curve.Count() >= 2);

            Contract.Ensures(Contract.Result<CurveProjectionResult3D>().Distance >= 0); 
            Contract.Ensures(Contract.Result<CurveProjectionResult3D>().SegmentIndex >= 0);
            Contract.Ensures(Contract.Result<CurveProjectionResult3D>().SegmentIndex < curve.Count() - 1); // segment index is less than num of segments
            // The projected point is closer to "point" than any other point on the curve.
            Contract.Ensures(Contract.ForAll(curve, curvePoint =>
                (curvePoint - point).Length >= Contract.Result<CurveProjectionResult3D>().Distance));

            var count = curve.Count();

            var projectedPoints =
                from indexedSegment in curve.SeqPairs().ZipIndex()
                let index = indexedSegment.Index
                let segment = indexedSegment.Value
                let segStart = segment.Item1
                let segEnd = segment.Item2
                let projectedPoint = point.ProjectOnSegment(segStart, segEnd)
                let distance = (point - projectedPoint).Length
                select new CurveProjectionResult3D(projectedPoint, index, distance);

            return projectedPoints.Minimizer(pair => pair.Distance);
        }

        /// <summary>
        /// Intersects a plane with a line
        /// </summary>
        /// <param name="plane">The plane to intersect with</param>
        /// <param name="p1">First point on the line</param>
        /// <param name="p2">Second point on the line</param>
        /// <returns>If the intersection exists - returns a value <c>t</c> such that the intersection point is 
        /// <c>t*p2 + (1-t)*p1</c>. If the intersection point doesn't exist, returns <c>NaN</c></returns>
        [Pure]
        public static double IntersectLine(this Plane3D plane, Point3D p1, Point3D p2)
        {
            var l = p2 - p1;
            var t = (plane.D - Vector3D.DotProduct((Vector3D)p1, plane.Normal)) / Vector3D.DotProduct(l, plane.Normal);
            return t;
        }

        /// <summary>
        /// Returns the distance of a point from a plane.
        /// </summary>
        /// <param name="plane">The plane</param>
        /// <param name="p">The point</param>
        /// <returns>The distance of the point from the plane</returns>
        public static double DistanceFromPoint(this Plane3D plane, Point3D p)
        {
            var dominator = Vector3D.DotProduct(p - Origin, plane.Normal) - plane.D;
            var denominator = plane.Normal.Length;
            return Math.Abs(dominator / denominator);
        }

        /// <summary>
        /// Projects a point on the given plane.
        /// </summary>
        /// <param name="plane">The plane to project on</param>
        /// <param name="p">The point to project</param>
        /// <returns>The projection of the point on the plane.</returns>
        public static Point3D ProjectPoint(this Plane3D plane, Point3D p)
        {
            var dominator = Vector3D.DotProduct(p - Origin, plane.Normal) - plane.D;
            var denumerator = plane.Normal.LengthSquared;
            var t = -dominator / denumerator;

            return p - t * plane.Normal;
        }

        public static Matrix3D LookAt(Point3D eye, Vector3D lookDirection, Vector3D upVector)
        {
            /*
            zaxis = normal(At - Eye)
            xaxis = normal(cross(Up, zaxis))
            yaxis = cross(zaxis, xaxis)

             xaxis.x           yaxis.x           zaxis.x          0
             xaxis.y           yaxis.y           zaxis.y          0
             xaxis.z           yaxis.z           zaxis.z          0
            -dot(xaxis, eye)  -dot(yaxis, eye)  -dot(zaxis, eye)  l
             * */

            var zaxis = lookDirection.Normalized();
            var xaxis = Vector3D.CrossProduct(upVector, zaxis).Normalized();
            var yaxis = Vector3D.CrossProduct(zaxis, xaxis);

            var vEye = (Vector3D)eye;

            return new Matrix3D(
                xaxis.X, yaxis.X, zaxis.X, 0, 
                xaxis.Y, yaxis.Y, zaxis.Y, 0, 
                xaxis.Z, yaxis.Z, zaxis.Z, 0, 
                -Vector3D.DotProduct(xaxis, vEye), -Vector3D.DotProduct(yaxis, vEye), -Vector3D.DotProduct(zaxis, vEye), 1);

        }

        public static Vector3D NormalVector(Vector3D v)
        {   
            var result = new Vector3D(-v.Z, 0, v.X);
            if (result == ZeroVector)
                result = new Vector3D(v.Y, -v.X, 0);

            if (result == ZeroVector)
                return result;
            else
            {
                result.Normalize();
                return result;
            }
        }

        public static Vector3D NextVector3D(this Random random)
        {
            return new Vector3D(random.NextDouble() - 0.5, random.NextDouble() - 0.5, random.NextDouble() - 0.5);
        }
    }
}
