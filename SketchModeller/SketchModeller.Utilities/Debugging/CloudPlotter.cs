using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SketchModeller.Utilities.Debugging
{
    class CloudPlotter : PointsPlotter
    {
        public CloudPlotter(double width, double height, Tuple<double, double> xRange, Tuple<double, double> yRange)
            : base(width, height, xRange, yRange)
        {
        }

        protected override Canvas Plot(Point[][] pointArrays, double xMin, double xMax, double yMin, double yMax)
        {
            var canvas = new Canvas
            {
                Width = width,
                Height = height,
                Background = Brushes.White,
            };

            foreach (var singleSequence in pointArrays)
            {
                var transformedPoints = from pnt in singleSequence
                                        let x = width * (pnt.X - xMin) / (xMax - xMin)
                                        let y = height * (pnt.Y - yMin) / (yMax - yMin)
                                        select new Point(x, y);

                var cloud = GeneratePoinsCloud(transformedPoints);
                cloud.Stroke = Brushes.DarkSlateBlue;
                cloud.StrokeThickness = 2.0;
                canvas.Children.Add(cloud);
            }

            return canvas;
        }

        private Path GeneratePoinsCloud(IEnumerable<Point> points)
        {
            var geometry = new GeometryGroup 
            { 
                Children = new GeometryCollection(from pnt in points
                                                  select new EllipseGeometry(pnt, 1, 1)),
            };
            geometry.Freeze();

            var path = new Path { Data = geometry };
            return path;
        }
    }
}
