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
        public static PointsPlotter Lines(
            double width = 256,
            double height = 256,
            Tuple<double, double> xRange = null,
            Tuple<double, double> yRange = null)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Ensures(Contract.Result<PointsPlotter>() != null);

            return new LinesPlotter(width, height, xRange, yRange);
        }

        public static PointsPlotter Cloud(
            double width = 256,
            double height = 256,
            Tuple<double, double> xRange = null,
            Tuple<double, double> yRange = null)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);
            Contract.Ensures(Contract.Result<PointsPlotter>() != null);

            return new CloudPlotter(width, height, xRange, yRange);
        }
    }
}
