using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedStraightGenCylinder : SnappedCylindricalPrimitive
    {
        public SnappedCyliderComponent[] Components { get; set; }
        public CylinderComponent[] ComponentResults { get; set; }

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
