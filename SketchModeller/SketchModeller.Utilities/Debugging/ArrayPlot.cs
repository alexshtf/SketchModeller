using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Utils;

namespace SketchModeller.Utilities.Debugging
{
    public static class ArrayPlot
    {
        public static FrameworkElement Lines(
            Point[][] lines,
            double width = 256,
            double height = 256,
            Tuple<double, double> xRange = null,
            Tuple<double, double> yRange = null)
        {
            Contract.Requires(lines != null);
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Ensures(Contract.Result<FrameworkElement>() != null);

            double xMin, xMax, yMin, yMax;
            ExtractRange(lines.Flatten(), xRange, p => p.X, out xMin, out xMax);
            ExtractRange(lines.Flatten(), yRange, p => p.Y, out yMin, out yMax);

            var canvas = new Canvas
            {
                Width = width,
                Height = height,
                Background = Brushes.White
            };
            foreach (var singleLine in lines)
            {
                var transformedPoints = from pnt in singleLine
                                        let x = width * (pnt.X - xMin) / (xMax - xMin)
                                        let y = height * (pnt.Y - yMin) / (yMax - yMin)
                                        select new Point(x, y);
                var polyline = new Polyline
                {
                    Points = new PointCollection(transformedPoints),
                    StrokeThickness = 2.0,
                    Stroke = Brushes.DarkSlateBlue,
                };
                canvas.Children.Add(polyline);
            }

            FWElementHelper.FakeLayout(canvas);
            return canvas;
        }

        private static void ExtractRange(
            IEnumerable<Point> points,
            Tuple<double, double> userRange,
            Func<Point, double> valueExtract,
            out double xMin, out double xMax)
        {
            if (userRange == null)
            {
                xMin = points.Select(valueExtract).Min();
                xMax = points.Select(valueExtract).Max();
            }
            else
            {
                xMin = userRange.Item1;
                xMax = userRange.Item2;
            }
        }
    }
}
