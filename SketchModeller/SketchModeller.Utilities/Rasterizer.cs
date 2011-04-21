using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading;
using Utils;

namespace SketchModeller.Utilities
{
    public static class Rasterizer
    {

        public static int[,] Rasterize(IEnumerable<Point> points, int width, int height)
        {
            int[,] result = new int[width, height];
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                    result[x, y] = -1;

            foreach (var pair in points.SeqPairs())
                Plot((int)Math.Round(pair.Item1.X), 
                     (int)Math.Round(pair.Item2.X), 
                     (int)Math.Round(pair.Item1.Y), 
                     (int)Math.Round(pair.Item2.Y), 
                     result);

            return result;

        }

        private static void Plot(int x0, int x1, int y0, int y1, int[,] result)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;
            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            for (int x = x0; x <= x1; ++x)
            {
                if (steep)
                    PutPixel(result, y, x);
                else
                    PutPixel(result, x, y);

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }
        }

        private static void PutPixel(int[,] image, int x, int y)
        {
            if (x >= 0 && x < image.GetLength(0) &&
                y >= 0 && y < image.GetLength(1))
                image[x, y] = 0;
        }

        private static void Swap(ref int x, ref int y)
        {
            var temp = y;
            y = x;
            x = temp;
        }
    }
}
