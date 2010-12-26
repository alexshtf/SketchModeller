using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data
{
    public static class DataExtensions
    {
        [Pure]
        public static System.Windows.Media.Media3D.Point3D ToWpfPoint(this Point3D p)
        {
            return new System.Windows.Media.Media3D.Point3D
            {
                X = p.X,
                Y = p.Y,
                Z = p.Z,
            };
        }

        [Pure]
        public static System.Windows.Point ToWpfPoint(this Point p)
        {
            return new System.Windows.Point
            {
                X = p.X,
                Y = p.Y,
            };
        }

        [Pure]
        public static System.Windows.Media.Media3D.Vector3D ToWpfVector(this Point3D p)
        {
            return (System.Windows.Media.Media3D.Vector3D)p.ToWpfPoint();
        }

        [Pure]
        public static System.Windows.Vector ToWpfVector(this Point p)
        {
            return (System.Windows.Vector)p.ToWpfPoint();
        }

        [Pure]
        public static Point3D ToDataPoint(this System.Windows.Media.Media3D.Point3D point)
        {
            return new Point3D { X = point.X, Y = point.Y, Z = point.Z };
        }

        [Pure]
        public static Point3D ToDataPoint(this System.Windows.Media.Media3D.Vector3D vector)
        {
            return new Point3D { X = vector.X, Y = vector.Y, Z = vector.Z };
        }
    }
}
