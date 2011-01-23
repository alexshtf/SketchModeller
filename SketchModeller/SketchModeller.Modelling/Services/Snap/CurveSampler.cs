using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

using WpfPoint = System.Windows.Point;
using PathGeometry = System.Windows.Media.PathGeometry;
using PathFigure = System.Windows.Media.PathFigure;
using PolyLineSegment = System.Windows.Media.PolyLineSegment;

namespace SketchModeller.Modelling.Services.Snap
{
    static class CurveSampler
    {
        public static Point[] UniformSample(PointsSequence pointsSequence, int count)
        {
            bool isClosed = false;
            if (pointsSequence is Polygon)
                isClosed = true;

            // segment of all points except first
            var polyLineSegment = new PolyLineSegment(pointsSequence.Points.Skip(1).Select(x => x.ToWpfPoint()), true); 

            // figure of all points
            var pathFigure = new PathFigure(pointsSequence.Points[0].ToWpfPoint(), Enumerable.Repeat(polyLineSegment, 1), isClosed);

            // geometry of a single figure
            var pathGeometry = new PathGeometry(Enumerable.Repeat(pathFigure, 1));

            var samples = new Point[count];
            for (int i = 0; i < count; ++i)
            {
                double fraction = (double)i / (double)(count);
                WpfPoint point;
                WpfPoint tangent;
                pathGeometry.GetPointAtFractionLength(fraction, out point, out tangent);
                samples[i] = new Point { X = point.X, Y = point.Y };
            }

            return samples;
        }
    }
}
