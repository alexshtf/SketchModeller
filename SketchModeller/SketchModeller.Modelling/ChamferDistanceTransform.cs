using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Windows;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling
{
    public static class ChamferDistanceTransform
    {
        // define chamfer values
        private const double a1 = 2.2062;
        private const double a2 = 1.4141;
        private const double a3 = 0.9866;

        private static readonly double[] FORWARD_LDM = { a1, a1, a1, a2, a3, a2, a1, a3, 0 };
        private static readonly int[] FORWARD_DX = { -2, -2, -1, -1, -1, -1, -1, 0, 0 };
        private static readonly int[] FORWARD_DY = { -1, 1, -2, -1, 0, 1, 2, -1, 0 };

        private static readonly double[] BACK_LDM = { 0, a3, a1, a2, a3, a2, a1, a1, a1 };
        private static readonly int[] BACK_DX = { 0, 0, 1, 1, 1, 1, 1, 2, 2 };
        private static readonly int[] BACK_DY = { 0, 1, -2, -1, 0, 1, 2, -1, 1 };

        public static void Compute(IEnumerable<Point> polyline, double[,] transform)
        {
            var width = transform.GetLength(0);
            var height = transform.GetLength(1);

            var raster = Rasterizer.Rasterize(polyline, width, height);
            Compute(raster, transform);
        }

        public static void Compute(int[,] image, double[,] result)
        {
            Contract.Requires(image != null);
            Contract.Requires(result != null);
            Contract.Requires(image.GetLength(0) == result.GetLength(0));
            Contract.Requires(image.GetLength(1) == result.GetLength(1));

            var width = image.GetLength(0);
            var height = image.GetLength(1);

            // initialize a distance-transform array with "infinity" values.
            var dt = new double[width + 4, height + 4];
            for (int j = 0; j < height + 4; ++j)
                for (int i = 0; i < width + 4; ++i)
                    dt[i, j] = double.PositiveInfinity;

            // put zero distance at feature pixels
            for (int j = 0; j < height; ++j)
                for (int i = 0; i < width; ++i)
                    if (image[i, j] == 0)
                        dt[i + 2, j + 2] = 0;

            // forward scan
            var LDM = FORWARD_LDM;
            var DX = FORWARD_DX;
            var DY = FORWARD_DY;
            for (int i = 2; i <= width + 1; ++i)
                for (int j = 2; j <= height + 1; ++j)
                {
                    var d0 = dt[i, j];
                    for (int k = 0; k < 9; ++k)
                    {
                        var r = i + DX[k];
                        var c = j + DY[k];
                        var d = dt[r, c] + LDM[k];
                        d0 = Math.Min(d, d0);
                    }
                    dt[i, j] = d0;
                }

            // backward scan
            LDM = BACK_LDM;
            DX = BACK_DX;
            DY = BACK_DY;
            for (int i = width + 1; i >= 2; --i)
                for (int j = height + 1; j >= 2; --j)
                {
                    var d0 = dt[i, j];
                    for (int k = 0; k < 9; ++k)
                    {
                        var r = i + DX[k];
                        var c = j + DY[k];
                        var d = dt[r, c] + LDM[k];
                        d0 = Math.Min(d, d0);
                    }
                    dt[i, j] = d0;
                }

            // create the result image - copy from DT the relevant sub-image
            for (int j = 0; j < height; ++j)
                for (int i = 0; i < width; ++i)
                    result[i, j] = dt[i + 2, j + 2];
        }
    }
}
