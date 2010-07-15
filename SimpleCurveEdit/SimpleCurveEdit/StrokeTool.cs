using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Petzold.Media3D;

namespace SimpleCurveEdit
{
    class StrokeTool : ITool
    {
        private readonly Polyline strokePolyline;
        private readonly Viewport3D viewport3d;
        private readonly ModelVisual3D curves;
        private bool isActive;

        public StrokeTool(Polyline strokePolyline, Viewport3D viewport3d, ModelVisual3D curves)
        {
            this.strokePolyline = strokePolyline;
            this.viewport3d = viewport3d;
            this.curves = curves;
        }

        public void MouseDown(Point position)
        {
            isActive = true;
            strokePolyline.Points = new PointCollection();
            strokePolyline.Points.Add(position);
        }

        public void MouseMove(Point position)
        {
            if (isActive)
                strokePolyline.Points.Add(position);
        }

        private Point3D Get3DPoint(Point point)
        {
            LineRange lineRange;
            if (ViewportInfo.Point2DtoPoint3D(viewport3d, point, out lineRange))
                return lineRange.PointFromZ(0); // the intersection of the ray with the plane Z=0
            else
                throw new InvalidOperationException("Cannot un-project the specified point. The point is invalid.");
        }

        public void MouseUp(Point position)
        {
            if (isActive)
            {
                isActive = false;
                if (strokePolyline.Points.Count > 1)
                {
                    var width = viewport3d.ActualWidth;
                    var height = viewport3d.ActualHeight;

                    var points3d = from point2d in strokePolyline.Points
                                   let pnt = Get3DPoint(point2d) // new Point3D(point2d.X - width / 2, -point2d.Y + height / 2, 0)
                                   select curves.Transform.Inverse.Transform(pnt);

                    var curve3d = new WirePolylineCurve();
                    curve3d.Points = new Point3DCollection(points3d);
                    curve3d.Thickness = 2;
                    curve3d.Color = Colors.Blue;
                    curves.Children.Add(curve3d);

                    strokePolyline.Points = null;
                }
            }
        }
    }
}
