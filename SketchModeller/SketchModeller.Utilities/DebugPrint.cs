using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Utilities
{
    public static class DebugPrint
    {
        public static string ToMatlab(this Tuple<Point, Point> pts)
        {
            var ptsArray = new Point[] { pts.Item1, pts.Item2 };
            return ToMatlab(ptsArray);
        }

        private static string ToMatlab(this IEnumerable<Point> points)
        {
            if (!points.Any())
                return "[]";

            const string FORMAT_FIRST = "{0}, {1};";
            const string FORMAT_REST = "\n  {0}, {1};";
            
            // put the first point
            var sb = new StringBuilder("[ ");
            sb.AppendFormat(FORMAT_FIRST, points.First().X, points.First().Y);
            
            // put the rest of the points
            foreach (var pnt in points.Skip(1))
                sb.AppendFormat(FORMAT_REST, pnt.X, pnt.Y);

            // remove the ';' character at the end
            sb.Remove(sb.Length - 1, 1);
            sb.Append(" ]");

            return sb.ToString();
        }
    }
}
