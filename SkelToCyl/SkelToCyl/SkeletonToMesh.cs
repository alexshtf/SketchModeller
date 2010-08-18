using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SkelToCyl
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
            List<int> indices = new List<int>();

            Vector3D u = GetVectorInFirstPlane(skeleton);
            foreach (var pair in skeleton.ZipIndex())
            {
                // find orthonormal basis for the plane at skelPoint
                u = ProjectOnPlane(u, Normalized(skelPoint.Normal), skelPoint.Position);
                Vector3D v = Vector3D.CrossProduct(u, skelPoint.Normal);
                u.Normalize();
                v.Normalize();

                // create a circle around the skeleton point
                for (int i = 0; i < slices; ++i)
                {
                    var angle = 2 * i * Math.PI / slices;
                    var x = skelPoint.Radius * Math.Cos(angle);
                    var y = skelPoint.Radius * Math.Sin(angle);

                    var pnt = skelPoint.Position + x * u + y * v; // create a point in the plane, using its orthonormal basis
                    positions.Add(pnt);
                }
            }

            Vector3D[] normals = null;
            int[] indices = null;
            return Tuple.Create(positions.ToArray(), normals, indices);
        }

        private static Vector3D Normalized(Vector3D input)
        {
            input.Normalize();
            return input;
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
