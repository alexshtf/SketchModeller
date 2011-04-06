using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;

using Enumerable = System.Linq.Enumerable;
using System.Threading.Tasks;

namespace SketchModeller.Modelling
{
    public static class DistanceTransform
    {
        public static byte[] ToGrayscale(double[,] doubleImage)
        {
            var width = doubleImage.GetLength(0);
            var height = doubleImage.GetLength(1);
            var imageValues =
                from y in Enumerable.Range(0, height)
                from x in Enumerable.Range(0, width)
                select doubleImage[x, y];

            var min = imageValues.Min();
            var max = imageValues.Max();

            var result =
                from x in imageValues
                let scaled = (x - min) / (max - min)
                select (byte)Math.Round(255 * scaled);

            return result.ToArray();
        }

        public static void Compute(IEnumerable<Point> polyline, double[,] transform)
        {
            var arrayPolyline = polyline.ToArray();
            var width = transform.GetLength(0);
            var height = transform.GetLength(1);
            Parallel.For(0, width, x =>
            {
                for (int y = 0; y < height; ++y)
                {
                    var pnt = new Point(x, y);
                    transform[x, y] = ShortestDistance(pnt, arrayPolyline);
                }
            });
        }

        private static double ShortestDistance(Point pnt, Point[] arrayPolyline)
        {
            double shortestDistance = double.MaxValue;
            for (int i = 0; i < arrayPolyline.Length - 1; ++i)
            {
                var curr = arrayPolyline[i];
                var next = arrayPolyline[i + 1];
                var dist = (pnt - pnt.ProjectOnSegment(curr, next)).Length;
                if (dist < shortestDistance)
                    shortestDistance = dist;
            }
            return shortestDistance;
        }
    }
}
