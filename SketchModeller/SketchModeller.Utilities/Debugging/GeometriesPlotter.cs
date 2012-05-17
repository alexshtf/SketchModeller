using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace SketchModeller.Utilities.Debugging
{
    public class GeometriesPlotter
    {
        private readonly double width;
        private readonly double height;

        public GeometriesPlotter(double width, double height)
        {
            this.width = width;
            this.height = height;
        }

        public FrameworkElement Plot(params Geometry[] geometries)
        {
            // compute values needed for constructing the visual tree of the plot
            var combinedGeometry = new GeometryGroup
            {
                Children = new GeometryCollection(geometries.Select(x => x.Clone()))
            };
            combinedGeometry.Freeze();
            var transform = ComputeTransform(combinedGeometry.Bounds);

            // construct the plot's visual stree.
            var canvas = new Canvas {
                Width = width,
                Height = height,
                Background = Brushes.White,
                Children = {
                    new Path {
                        Data = combinedGeometry,
                        LayoutTransform = transform,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1,
                    }
                }
            };

            return canvas;
        }

        /// <summary>
        /// Computes a transform that maps the bounding box to the rectangle (0, 0, width, height)
        /// </summary>
        /// <param name="boundingBox">The bounding box</param>
        /// <returns>The transform</returns>
        private Transform ComputeTransform(Rect boundingBox)
        {
            var translateTransform = new TranslateTransform(-boundingBox.X, -boundingBox.Y);
            var scaleTransform = new ScaleTransform(width / boundingBox.Width, height / boundingBox.Height);
            return new TransformGroup { Children = new TransformCollection { translateTransform, scaleTransform } };
        }
    }
}
