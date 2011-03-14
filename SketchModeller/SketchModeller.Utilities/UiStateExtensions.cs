using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using Petzold.Media3D;
using Utils;
using System.Windows;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public static class UiStateExtensions
    {
        /// <summary>
        /// Projects a 3D points to 2D coordinates
        /// </summary>
        /// <param name="sketchPlane">The sketch plane that defined projection parameters</param>
        /// <param name="point3d">The point to project</param>
        /// <returns>The projected point's coordinates</returns>
        public static Point Project(this SketchPlane sketchPlane, Point3D point3d)
        {
            Contract.Requires(sketchPlane != null);

            var xCoord = Vector3D.DotProduct(point3d - sketchPlane.Center, sketchPlane.XAxis);
            var yCoord = Vector3D.DotProduct(point3d - sketchPlane.Center, sketchPlane.YAxis);
            return new Point(xCoord, -yCoord);
        }

        /// <summary>
        /// Projects a 3D term point to 2D coordinates
        /// </summary>
        /// <param name="sketchPlane">The sketch plane that defines the projection parameters</param>
        /// <param name="point3d">The point to project</param>
        /// <returns>The projected point's coordinates</returns>
        public static TVec Project(this SketchPlane sketchPlane, TVec point3d)
        {
            Contract.Requires(sketchPlane != null);
            Contract.Requires(point3d != null);
            Contract.Requires(point3d.Dimension == 3);
            Contract.Ensures(Contract.Result<TVec>() != null);
            Contract.Ensures(Contract.Result<TVec>().Dimension == 2);

            var center = new TVec(sketchPlane.Center.X, sketchPlane.Center.Y, sketchPlane.Center.Z);
            var xAxis = new TVec(sketchPlane.XAxis.X, sketchPlane.XAxis.Y, sketchPlane.XAxis.Z);
            var yAxis = new TVec(sketchPlane.YAxis.X, sketchPlane.YAxis.Y, sketchPlane.YAxis.Z);

            var centered = point3d - center;
            var xCoord = centered * xAxis;
            var yCoord = centered * yAxis;
            return new TVec(xCoord, yCoord);
        }

        /// <summary>
        /// Gets the point on the sketch plane that is intersected by a given ray
        /// </summary>
        /// <param name="sketchPlane">The sketch plane</param>
        /// <param name="lineRange">The ray information</param>
        /// <returns>The point on the plane of <paramref name="sketchPlane"/> that intersects the ray given by <paramref name="lineRange"/>, or <c>null</c>
        /// if no such point exists.</returns>
        public static Point3D? PointFromRay(this SketchPlane sketchPlane, LineRange lineRange)
        {
            Contract.Requires(sketchPlane != null);

            var plane = Plane3D.FromPointAndNormal(sketchPlane.Center, sketchPlane.Normal);
            var t = plane.IntersectLine(lineRange.Point1, lineRange.Point2);
            if (double.IsNaN(t))
                return null;
            else
                return MathUtils3D.Lerp(lineRange.Point1, lineRange.Point2, t);
        }
    }
}
