using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Utils;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    /// <summary>
    /// A cylinder with constrained vertices that will not move during smoothing steps
    /// </summary>
    class ConstrainedCylinder : ConstrainedMesh
    {
        public const double EPSILON = 1E-5;

        private const double LENGTH_LINKS_FACTOR = 0.1;
        private const int MIN_LINKS = 2;

        private const double RADIUS_SLICES_FACTOR = 0.1;
        private const int MIN_SLICES = 10;

        public ConstrainedCylinder(double radius, double length, Point3D center, Vector3D orientation, Vector3D viewDirection)
        {
            Contract.Requires(radius > 0);
            Contract.Requires(length > 0);
            Contract.Requires(orientation != MathUtils3D.ZeroVector);
            Contract.Requires(!MathUtils3D.AreParallel(orientation, viewDirection));

            Contract.Ensures(Positions.Count > 0);
            Contract.Ensures(Normals.Count > 0);
            Contract.Ensures(TriangleIndices.Count > 0);
            Contract.Ensures(ConstrainedPositionIndices.Count > 0);

            /*
             * We will generate the cylinder by gluing together circles around many centers. Each such center is referred to as
             * a "link" (a link in the chain along the cylinder's skeletal line). Each circle is approximated by a regular polygon
             * and we refer to its edges as slices. The circle will be generated using an orthonormal basis for the plane it resides in,
             * and the center of the circle is this plane's origin.
             * */

            // we will need a normalized orientation vector. We use it to go small steps which
            // eventually will sum up to the length of the cylinder.
            orientation.Normalize();

            // calculate the number of links and slices
            var slicesCount = Math.Max(MIN_SLICES, (int)Math.Round(radius * RADIUS_SLICES_FACTOR));
            var linksCount = Math.Max(MIN_LINKS, (int)Math.Round(1 + length * LENGTH_LINKS_FACTOR));

            // slices coun must be even, so that we have two "half circles" 
            // such that the first vertex of each half-circle is constrained.
            slicesCount = slicesCount % 2 == 0 ? slicesCount : slicesCount + 1;

            // calculate the orthonormal basis for the planes. This spanning basis remains the same for all circles.
            var u = Vector3D.CrossProduct(viewDirection, orientation);
            var v = Vector3D.CrossProduct(u, orientation);
            u.Normalize();
            v.Normalize();

            // starting position is half the length before the center (ending position will be half the way after).
            var start = center - orientation * length / 2;
            
            // calculate the first circle's positions and normals
            for (int i = 0; i < linksCount; ++i)
            {
                var circleCenterDistance = length * i / (double)(linksCount - 1);
                var circleCenter = center + circleCenterDistance * orientation;

                var circle = GenerateCircle(circleCenter, u, v, radius, slicesCount);
                var circleIndices = System.Linq.Enumerable.Range(Positions.Count, circle.Length).ToArray();
                foreach(var item in circle)
                {
                    Positions.Add(item.Position);
                    Normals.Add(item.Normal);
                }

                if (i == 0 || i == linksCount - 1) // first and last circles are always constrained
                    ConstrainedPositionIndices.AddRange(circleIndices);
                else // only the two half-circle start points are constrained
                {
                    var c1 = circleIndices[0];
                    var c2 = circleIndices[1 + circleIndices.Length / 2];
                    ConstrainedPositionIndices.AddMany(c1, c2);
                }

                if (i > 0) // starting at the second circle - we can add triangle indices
                {
                    var prevCircleIndices = circleIndices.Select(x => x - slicesCount).ToArray();
                    for (var currIdx = 0; currIdx < slicesCount; ++currIdx)
                    {
                        var nextIdx = (currIdx + 1) % slicesCount;

                        // create indices of two triangles for the current quad
                        var t1 = Tuple.Create(
                            prevCircleIndices[currIdx],
                            circleIndices[currIdx],
                            circleIndices[nextIdx]);
                        var t2 = Tuple.Create(
                            prevCircleIndices[currIdx],
                            circleIndices[nextIdx],
                            prevCircleIndices[nextIdx]);

                        TriangleIndices.AddMany(t1, t2);
                    }
                }
            }
        }

        private static PositionNormal[] GenerateCircle(Point3D center, Vector3D u, Vector3D v, double radius, int slicesCount)
        {
            Contract.Requires(slicesCount > 3); // we have a valid slices count (at-least a triangle)
            Contract.Requires(radius > 0); 

            // u, v form an orthonormal basis
            Contract.Requires(MathUtils3D.AreOrthogonal(u, v));
            Contract.Requires(Math.Abs(u.LengthSquared - 1) < EPSILON);
            Contract.Requires(Math.Abs(v.LengthSquared - 1) < EPSILON);

            Contract.Ensures(Contract.Result<PositionNormal[]>().Length == slicesCount); // we indeed produced the correct slices count
            Contract.Ensures(Contract.ForAll(Contract.Result<PositionNormal[]>(), pointNormal => 
                Math.Abs(radius - (pointNormal.Position - center).Length) < EPSILON)); // all points on the circle are indeed radius-far from the center.

            var result = new PositionNormal[slicesCount];
            for (int i = 0; i < slicesCount; ++i)
            {
                var angle = 2 * Math.PI * i / (double)(slicesCount - 1);
                result[i].Normal = Math.Cos(angle) * u + Math.Sin(angle) * v;
                result[i].Position = center + radius * result[i].Normal;
            }

            return result;
        }

        private struct PositionNormal
        {
            public Point3D Position;
            public Vector3D Normal;
        }
    }
}
