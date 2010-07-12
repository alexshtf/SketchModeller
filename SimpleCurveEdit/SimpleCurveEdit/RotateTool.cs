using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using Utils;

namespace SimpleCurveEdit
{
    class RotateTool : ITool
    {
        private const double ROTATION_SPEED = 0.1;

        private bool isActive;
        private Point prevPosition;
        private MatrixTransform3D curvesTransform;
        private ModelVisual3D curves;

        public RotateTool(MatrixTransform3D curvesTransform, ModelVisual3D curves)
        {
            this.curvesTransform = curvesTransform;
            this.curves = curves;
        }

        public void MouseDown(Point position)
        {
            isActive = true;
            prevPosition = position;
        }

        public void MouseMove(Point position)
        {
            if (isActive)
            {
                var angle = (position.X - prevPosition.X) * ROTATION_SPEED;
                var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), angle);
                var transform = new RotateTransform3D(rotation);

                curvesTransform.Matrix = curvesTransform.Matrix * transform.Value;

                //foreach (var curve3d in curves.VisualTree().OfType<Curve3D>())
                //    curve3d.TryUpdateGeometry();

                prevPosition = position;
            }
        }

        public void MouseUp(Point position)
        {
            isActive = false;
        }
    }
}
