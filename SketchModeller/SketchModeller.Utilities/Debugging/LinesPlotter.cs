using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SketchModeller.Utilities.Debugging
{
    class LinesPlotter : PointsPlotter
    {
        public LinesPlotter(double width, double height, Tuple<double, double> xRange, Tuple<double, double> yRange)
            : base(width, height, xRange, yRange)
        {
        }

        protected override Canvas Plot(Point[][] pointArrays, double xMin, double xMax, double yMin, double yMax)
        {
            var canvas = new Canvas
            {
                Width = width,
                Height = height,
                Background = Brushes.White
            };
            foreach (var singleLine in pointArrays)
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

            return canvas;
        }
    }
}
