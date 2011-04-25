using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows;

namespace SketchModeller.Utilities
{
    public struct CircleParams
    {
        public Point Center;
        public double Radius;
    }

    public static class CircleFitter
    {
        public static CircleParams Fit(IList<Point> points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Count >= 3);
            Contract.Ensures(Contract.Result<CircleParams>().Radius > 0);

            var n = points.Count;

            var avgX = points.Select(p => p.X).Average();
            var avgY = points.Select(p => p.Y).Average();

            var u = points.Select(p => p.X - avgX).ToArray();
            var v = points.Select(p => p.Y - avgY).ToArray();

            double svv = 0;
            double suu = 0;
            double suv = 0;
            double suuu = 0;
            double svvv = 0;
            double suvv = 0;
            double svuu = 0;

            for (int i = 0; i < n; ++i)
            {
                svv += v[i] * v[i];
                suu += u[i] * u[i];
                suv += u[i] * v[i];
                suuu += u[i] * u[i] * u[i];
                svvv += v[i] * v[i] * v[i];
                suvv += u[i] * v[i] * v[i];
                svuu += v[i] * u[i] * u[i];
            }

            // calculate determinants according to Cramer's rule
            var det = suv * suv - suu * svv;
            var detu = 0.5 * svv * (suuu + suvv) - 0.5 * suv * (svuu + svvv);
            var detv = 0.5 * suu * (svuu + svvv) - 0.5 * suv * (suuu + suvv);

            // compute the center of the circle in (u, v) coordinates
            var uc = -detu / det;
            var vc = -detv / det;

            // compute the radius
            var rSq = uc * uc + vc * vc + (suu + svv) / n;
            var r = Math.Sqrt(rSq);

            return new CircleParams
            {
                Center = new Point(uc + avgX, vc + avgY),
                Radius = r,
            };
        }
    }
}
