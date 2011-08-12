using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data.EditConstraints
{
    [Serializable]
    public class PointOnPlaneConstraint : PrimitiveEditConstraint
    {
        private readonly Point3D planePoint;
        private readonly Vector3D planeNormal;
        private readonly PointParameter point;

        public PointOnPlaneConstraint(Point3D planePoint, Vector3D planeNormal, PointParameter point)
        {
            Contract.Requires(point != null);
            Contract.Ensures(PlanePoint == planePoint);
            Contract.Ensures(PlaneNormal == planeNormal);
            Contract.Ensures(Point == point);

            this.planePoint = planePoint;
            this.planeNormal = planeNormal;
            this.point = point;
        }

        public Point3D PlanePoint { get { return planePoint; } }
        public Vector3D PlaneNormal { get { return planeNormal; } }
        public PointParameter Point { get { return point; } }
    }
}
