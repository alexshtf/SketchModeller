using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Utils;

namespace MultiviewCurvesToCyl
{
    static class SkeletonToMesh
    {
        /// <summary>
        /// Constructs a mesh of a cylinder given its skeleton.
        /// </summary>
        /// <param name="skeleton">The 1D skeleton of the cylinder</param>
        /// <returns></returns>
        public static Tuple<Point3D[], Vector3D[], int[]> SkeletonToCylinder(IEnumerable<SkeletonPoint> skeleton, int slices)
        {
            List<Point3D> positions = new List<Point3D>();
            List<Vector3D> normals = new List<Vector3D>();
            List<int> indices = new List<int>();

            Vector3D u = GetVectorInFirstPlane(skeleton);
            var skelSize = skeleton.Count();
            foreach (var indexedSkelPoint in skeleton.ZipIndex())
            {
                var skelPoint = indexedSkelPoint.Value;
                var skelPointIdx = indexedSkelPoint.Index;

                var top = skelPointIdx * slices;
                var bot = (skelPointIdx + 1) * slices;

                // find orthonormal basis for the plane at skelPoint
                u = ProjectOnPlane(u, skelPoint.Normal.Normalized(), skelPoint.Position);
                Vector3D v = Vector3D.CrossProduct(u, skelPoint.Normal);
                u.Normalize();
                v.Normalize();

                // create a circle around the skeleton point
                for (int sliceIdx = 0; sliceIdx < slices; ++sliceIdx)
                {
                    // calculate coordinates of a circle point using the orthonormal basis.
                    var angle = 2 * sliceIdx * Math.PI / slices;
                    var x = skelPoint.Radius * Math.Cos(angle);
                    var y = skelPoint.Radius * Math.Sin(angle);

                    // create a point in the plane, using its orthonormal basis
                    var pnt = skelPoint.Position + x * u + y * v;
                    positions.Add(pnt);

                    // calculate normal at the point
                    var pointNormal = pnt - skelPoint.Position;
                    pointNormal.Normalize();
                    normals.Add(pointNormal);

                    // add mesh triangle indices
                    if (skelPointIdx < skelSize - 1)
                    {
                        indices.Add(top + sliceIdx);
                        indices.Add(bot + sliceIdx);
                        indices.Add(top + ((sliceIdx + 1) % slices));

                        indices.Add(top + ((sliceIdx + 1) % slices));
                        indices.Add(bot + sliceIdx);
                        indices.Add(bot + ((sliceIdx + 1) % slices));
                    }
                }
            }

            return Tuple.Create(positions.ToArray(), normals.ToArray(), indices.ToArray());
        }

        private static Vector3D GetVectorInFirstPlane(IEnumerable<SkeletonPoint> skeleton)
        {
            var first = skeleton.FirstOrDefault();
            return FindVectorInPlane(first.Position, first.Normal);
        }

        private static Vector3D FindVectorInPlane(Point3D pnt, Vector3D normal)
        {
            normal.Normalize();

            // find a vector not parallel to the normal
            var vec = normal;
            vec += new Vector3D(1, -1, 1);

            return ProjectOnPlane(vec, normal, pnt);
        }

        private static Vector3D ProjectOnPlane(Vector3D toProject, Vector3D normal, Point3D pnt)
        {
            var p = pnt + toProject;
            var proj = ProjectOnPlane(p, normal, pnt);
            return proj - pnt;
        }

        [Pure]
        private static double Substitute(Point3D toSubst, Vector3D normal, Point3D pnt)
        {
            return Vector3D.DotProduct((toSubst - pnt), normal);
        }

        private static Point3D ProjectOnPlane(Point3D toProject, Vector3D normal, Point3D pnt)
        {
            Contract.Requires(Math.Abs(normal.LengthSquared - 1) <= 1E-5); // normal is normalized and non-degenerate                        
            Contract.Ensures(Math.Abs(Substitute(Contract.Result<Point3D>(), normal, pnt)) <= 1E-5); // result on the plane

            var u = Vector3D.DotProduct(normal, toProject - pnt);
            var proj = new Point3D(
                toProject.X - normal.X * u,
                toProject.Y - normal.Y * u,
                toProject.Z - normal.Z * u);

            return proj;
        }
    }
}
