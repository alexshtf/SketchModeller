using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure;
using System.Windows.Media;
using SketchModeller.Utilities;
using Utils;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling
{
    static class DistanceTransformIntegral
    {
        public static double Compute(PointsSequence curve, int[,] distanceTransform)
        {
            var pnts = (from pnt in curve.Points
                        let x = 0.5 * Constants.DISTANCE_TRANSFORM_RESOLUTION * (pnt.X + 1)
                        let y = 0.5 * Constants.DISTANCE_TRANSFORM_RESOLUTION * (pnt.Y + 1)
                        select new Point(x, y)
                       ).ToArray();

            if (curve is Polygon)
                pnts = pnts.Append(pnts.First()).ToArray();

            double integral = 0;
            for (int i = 0; i < pnts.Length - 1; ++i)
            {
                var curr = pnts[i];
                var next = pnts[i + 1];
                bool truncated = i < pnts.Length - 2;
                integral += Compute(curr, next, distanceTransform, truncated);
            }

            return 0;
        }

        public static double Compute(Point segStart, Point segEnd, int[,] distanceTransform, bool truncated = true, double minSamplesInterval = 0.0625)
        {
            var segLen = (segEnd - segStart).Length;
            var numOfSamples = 1 + (int)Math.Ceiling(segLen / minSamplesInterval);

            Func<double, double> sample = t =>
                {
                    var pnt = WpfUtils.Lerp(segStart, segEnd, t);
                    return SampleImage(distanceTransform, pnt.X, pnt.Y);
                };

            double b = 1;
            if (truncated) // we discard the end-point
                b = ((double)numOfSamples - 1) / numOfSamples;

            var integral = Simpson(sample, 0, b, numOfSamples);
            return integral;
        }

        private static double SampleImage(int[,] img, double u, double v)
        {
            var width = img.GetLength(0);
            var height = img.GetLength(1);

            // ensure u,v don't go out of image space range
            u = u.Clamp(0, width - 1);
            v = v.Clamp(0, height - 1);

            // compute pixel integer coordinates
            int x = (int)Math.Floor(u);
            int y = (int)Math.Floor(v);

            var uRatio = u - x;
            var vRatio = v - y;
            var uOpposite = 1 - uRatio;
            var vOpposite = 1 - vRatio;
            
            // safely take image values - if we go out of range we return 0.
            Func<int, int, int> safeImg = (i, j) =>
            {
                if (i < width && j < height)
                    return img[i, j];
                else
                    return 0;
            };
            
            // bilinearly interpolate pixels
            var result = (safeImg(x, y) * uOpposite + safeImg(x + 1, y) * uRatio) * vOpposite +
                         (safeImg(x, y + 1) * uOpposite + safeImg(x + 1, y + 1) * uRatio) * vRatio;

            return result;
        }

        private static double Simpson(Func<double, double> f, double a, double b, int n)
        {
            n *= 2;
            double h = (b - a) / n;
            double s = f(a);

            for (int i = 1; i < n; i += 2)
            {
                double x = a + h * i;
                s += 4 * f(x);
            }

            for(int i = 2; i < n - 1; i += 2)
            {
                double x = a + h * i;
                s += 2 * f(x);
            }

            s += f(b);

            var result = h * s / 3;
            return result;
        }
    }
}
