using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace SketchModeller.Utilities
{
    public static class PolygonRasterizer
    {
        public static bool[,] Rasterize(Point[] points, int width, int height)
        {
            Contract.Requires(points != null);
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Requires(width % 8 == 0);
            Contract.Ensures(Contract.Result<bool[,]>() != null);
            Contract.Ensures(Contract.Result<bool[,]>().GetLength(0) == width);
            Contract.Ensures(Contract.Result<bool[,]>().GetLength(1) == height);

            var canvas = new Canvas { Background = Brushes.White, Width = width, Height = height };
            var polygon = new Polygon { Stroke = Brushes.Black, Fill = Brushes.Black, StrokeThickness = 1, Points = new PointCollection(points) };
            canvas.Children.Add(polygon);
            RenderOptions.SetEdgeMode(canvas, EdgeMode.Aliased);

            canvas.Measure(new Size(width, height));
            canvas.Arrange(new Rect(0, 0, canvas.DesiredSize.Width, canvas.DesiredSize.Height));

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(canvas);

            var fmb = new FormatConvertedBitmap(rtb, PixelFormats.BlackWhite, null, 0);
            var pixels = new byte[width * height / 8];
            fmb.CopyPixels(pixels, width / 8, 0);

            System.Collections.BitArray ba = new System.Collections.BitArray(pixels);

            var result = new bool[width, height];
            for (int i = 0, y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x, ++i)
                    result[x, y] = !ba[i];

            return result;
        }
    }
}
