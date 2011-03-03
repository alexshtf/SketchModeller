using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Utils
{
    /// <summary>
    /// Represents a plane in the 3D space with a plane equation &lt;N, x&gt; - D = 0 where N is a normal vector, 
    /// x is a point on the plane and D is a real number.
    /// </summary>
    public struct Plane3D
    {
        public readonly Vector3D Normal;
        public readonly double D;

        public Plane3D(Vector3D normal, double d)
        {
            Normal = normal;
            D = d;
        }

        public Plane3D(double a, double b, double c, double d)
            : this(new Vector3D(a, b, c), d)
        {
        }

        public static Plane3D FromPoints(Point3D p1, Point3D p2, Point3D p3)
        {
            var v1 = p2 - p1;
            var v2 = p3 - p1;
            return FromPointAndVectors(p1, v1, v2);
        }

        public static Plane3D FromPointAndVectors(Point3D p, Vector3D v1, Vector3D v2)
        {
            var normal = Vector3D.CrossProduct(v1, v2);
            return FromPointAndNormal(p, normal);
        }

        public static Plane3D FromPointAndNormal(Point3D p, Vector3D n)
        {
            var d = Vector3D.DotProduct(p - MathUtils3D.Origin,  n);
            return new Plane3D(n, d);
        }
    }
}
