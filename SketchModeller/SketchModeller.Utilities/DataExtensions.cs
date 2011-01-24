﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Utilities
{
    public static class DataExtensions
    {
        public static NewCylinderWpf ToWpf(this NewCylinder newCylinder)
        {
            return new NewCylinderWpf
            {
                Axis = newCylinder.Axis.ToWpfVector(),
                Center = newCylinder.Center.ToWpfPoint(),
                Length = newCylinder.Length,
                Diameter = newCylinder.Diameter,
            };
        }

        [Pure]
        public static IEnumerable<System.Windows.Media.Media3D.Point3D> ToWpfPoints(this IEnumerable<Point3D> ps)
        {
            return ps.Select(p => p.ToWpfPoint());
        }

        [Pure]
        public static IEnumerable<System.Windows.Point> ToWpfPoints(this IEnumerable<Point> ps)
        {
            return ps.Select(p => p.ToWpfPoint());
        }

        [Pure]
        public static IEnumerable<System.Windows.Media.Media3D.Vector3D> ToWpfVectors(this IEnumerable<Point3D> vs)
        {
            return vs.Select(v => v.ToWpfVector());
        }

        [Pure]
        public static IEnumerable<System.Windows.Vector> ToWpfVectors(this IEnumerable<Point> vs)
        {
            return vs.Select(v => v.ToWpfVector());
        }

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
        public static IEnumerable<Point3D> ToDataPoints(this IEnumerable<System.Windows.Media.Media3D.Point3D> ps)
        {
            return ps.Select(p => p.ToDataPoint());
        }

        [Pure]
        public static IEnumerable<Point3D> ToDataPoints(this IEnumerable<System.Windows.Media.Media3D.Vector3D> vs)
        {
            return vs.Select(v => v.ToDataPoint());
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
