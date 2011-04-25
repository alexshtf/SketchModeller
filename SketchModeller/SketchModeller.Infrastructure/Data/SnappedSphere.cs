using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    public class SnappedSphere : SnappedPrimitive
    {
        public SnappedSphere()
        {
            FeatureCurves = new FeatureCurve[1];
            FeatureCurves[0] = ProjectionParallelCircle = new CircleFeatureCurve();
            ProjectionParallelCircle.Normal = new TVec(0, 0, 1);
            ProjectionParallelCircle.NormalResult = new Vector3D(0, 0, 1);
        }

        public TVec Center { get; set; }
        public Variable Radius { get; set; }

        public Point3D CenterResult { get; set; }
        public double RadiusResult { get; set; }

        public override void UpdateFeatureCurves()
        {
            // update variables
            ProjectionParallelCircle.Center = Center;
            ProjectionParallelCircle.Radius = Radius;

            // update results
            ProjectionParallelCircle.CenterResult = CenterResult;
            ProjectionParallelCircle.RadiusResult = RadiusResult;
        }

        public CircleFeatureCurve ProjectionParallelCircle { get; private set; }
    }
}
