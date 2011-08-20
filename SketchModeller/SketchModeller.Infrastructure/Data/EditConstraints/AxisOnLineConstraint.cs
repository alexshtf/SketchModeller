using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data.EditConstraints
{
    [Serializable]
    public class AxisOnLineConstraint : PrimitiveEditConstraint
    {
        private readonly Point3D linePoint;
        private readonly Vector3D lineDirection;
        private readonly PointParameter axisPoint;
        private readonly VectorParameter axisDirection;

        public AxisOnLineConstraint(Point3D linePoint, Vector3D lineDirection, PointParameter axisPoint, VectorParameter axisDirection)
        {
            Contract.Requires(axisPoint != null);
            Contract.Requires(axisDirection != null);

            Contract.Ensures(LinePoint == linePoint);
            Contract.Ensures(LineDirection == lineDirection);
            Contract.Ensures(AxisPoint == axisPoint);
            Contract.Ensures(AxisDirection == axisDirection);

            this.linePoint = linePoint;
            this.lineDirection = lineDirection;

            this.axisPoint = axisPoint;
            this.axisDirection = axisDirection;
        }

        public Point3D LinePoint { get { return linePoint; } }
        public Vector3D LineDirection { get { return lineDirection; } }
        public PointParameter AxisPoint { get { return axisPoint; } }
        public VectorParameter AxisDirection { get { return axisDirection; } }
    }
}
