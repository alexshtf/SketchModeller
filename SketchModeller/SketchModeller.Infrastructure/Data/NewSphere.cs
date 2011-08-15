using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Infrastructure.Data
{
    public class NewSphere : NewPrimitive
    {
        public NewSphere()
        {
            FeatureCurves = new PrimitiveCurve[0];
            SilhouetteCurves = ArrayUtils.Generate<PrimitiveCurve>(1);
        }

        public PointParameter Center { get; private set; }
        public ValueParameter Radius { get; private set; }

        public PrimitiveCurve SilhouetteCircle
        {
            get { return SilhouetteCurves[0]; }
        }

        public override void UpdateCurvesGeometry()
        {
            var circle3d = ShapeHelper.GenerateCircle(Center, new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), Radius, 20);
            var circle = ShapeHelper.ProjectCurve(circle3d);

            SilhouetteCircle.Points = circle.Append(circle.First()).ToArray();
        }
    }
}
