using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedBendedGenCylinder : SnappedCylindricalPrimitive
    {
        public SnappedBendedCylinderComponent[] Components { get; set; }
        public BendedCylinderComponent[] ComponentResults { get; set; }

        public Point[] pntseq;

        public override void UpdateFeatureCurves()
        {
            base.UpdateFeatureCurves();

            BottomFeatureCurve.Radius = Components.First().Radius;
            BottomFeatureCurve.RadiusResult = ComponentResults.First().Radius;

            TopFeatureCurve.Radius = Components.Last().Radius;
            TopFeatureCurve.RadiusResult = ComponentResults.Last().Radius;
        }
    }
}