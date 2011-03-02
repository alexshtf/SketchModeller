using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using Petzold.Media3D;
using Utils;

namespace SketchModeller.Utilities
{
    public static class UiStateExtensions
    {
        public static Point3D? PointFromRay(this SketchPlane sketchPlane, LineRange lineRange)
        {
            var plane = Plane3D.FromPointAndNormal(sketchPlane.Center, sketchPlane.Normal);
            var t = plane.IntersectLine(lineRange.Point1, lineRange.Point2);
            if (double.IsNaN(t))
                return null;
            else
                return MathUtils3D.Lerp(lineRange.Point1, lineRange.Point2, t);
        }
    }
}
