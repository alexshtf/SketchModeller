using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SnappedCylinderConverter : BaseSnappedConverter<SnappedCylinder>
    {
        protected override NewPrimitive ConvertCore(SnappedCylinder snapped)
        {
            var result = new NewCylinder();
            result.Axis.Value = snapped.AxisResult;
            result.Length.Value = snapped.LengthResult;
            result.Diameter.Value = 2 * snapped.RadiusResult;
            result.Center.Value = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5);

            result.EditConstraints.Add(new AxisOnLineConstraint(
                snapped.BottomCenterResult,
                snapped.AxisResult,
                result.Center,
                result.Axis));
            return result;
        }
    }
}
