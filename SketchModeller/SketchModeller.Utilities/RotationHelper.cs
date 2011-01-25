using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace SketchModeller.Utilities
{
    public static class RotationHelper
    {
        public static Vector3D RotateVector(Vector3D vector, Vector3D rotateAxis, double degrees)
        {
            var transform = new RotateTransform3D(new AxisAngleRotation3D(rotateAxis, degrees));
            var result = transform.Transform(vector);
            return result;
        }
    }
}
