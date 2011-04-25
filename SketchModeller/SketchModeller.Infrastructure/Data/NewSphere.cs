using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    public class NewSphere : NewPrimitive
    {
        public NewSphere()
        {
            FeatureCurves = new PrimitiveCurve[0];
            SilhouetteCurves = ArrayUtils.Generate<PrimitiveCurve>(1);
        }

        #region Center property

        private Point3D center;

        public Point3D Center
        {
            get { return center; }
            set
            {
                center = value;
                RaisePropertyChanged(() => Center);
            }
        }

        #endregion

        #region Radius property

        private double radius;

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                RaisePropertyChanged(() => Radius);
            }
        }

        #endregion

        public PrimitiveCurve SilhouetteCircle
        {
            get { return SilhouetteCurves[0]; }
        }

        public override void UpdateCurvesGeometry()
        {
            var circle3d = ShapeHelper.GenerateCircle(center, new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), radius, 20);
            var circle = ShapeHelper.ProjectCurve(circle3d);

            SilhouetteCircle.Points = circle.Append(circle.First()).ToArray();
        }
    }
}
