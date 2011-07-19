using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;
using Utils;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SketchModeller.Utilities.Debugging
{
    public abstract class PointsPlotter
    {
        protected readonly double width;
        protected readonly double height;
        protected readonly Tuple<double, double> xRange;
        protected readonly Tuple<double, double> yRange;

        public PointsPlotter(double width, double height, Tuple<double, double> xRange, Tuple<double, double> yRange)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);

            this.width = width;
            this.height = height;
            this.xRange = xRange;
            this.yRange = yRange;
        }

        public FrameworkElement Plot(params Point[][] pointArrays)
        {
            Contract.Requires(pointArrays != null);
            Contract.Ensures(Contract.Result<FrameworkElement>() != null);

            double xMin, xMax, yMin, yMax;
            ExtractRange(pointArrays.Flatten(), xRange, p => p.X, out xMin, out xMax);
            ExtractRange(pointArrays.Flatten(), yRange, p => p.Y, out yMin, out yMax);

            var canvas = Plot(pointArrays, xMin, xMax, yMin, yMax);
            FWElementHelper.FakeLayout(canvas);

            return canvas;
        }

        protected abstract Canvas Plot(Point[][] pointArrays, double xMin, double xMax, double yMin, double yMax);

        private static void ExtractRange(
            IEnumerable<Point> points,
            Tuple<double, double> userRange,
            Func<Point, double> valueExtract,
            out double min, out double max)
        {
            if (userRange == null)
            {
                min = points.Select(valueExtract).Min();
                max = points.Select(valueExtract).Max();
            }
            else
            {
                min = userRange.Item1;
                max = userRange.Item2;
            }
        }
    }
}
