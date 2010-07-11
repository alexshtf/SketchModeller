using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Controls;

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
                                   let pnt = new Point3D(point2d.X - width / 2, -point2d.Y + height / 2, 0)
                                   select curves.Transform.Inverse.Transform(pnt);

                    var curve3d = new Curve3D();
                    curve3d.Positions = new Point3DCollection(points3d);
                    curves.Children.Add(curve3d);

                    strokePolyline.Points = null;
                }
            }
        }
    }
}
