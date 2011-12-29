using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;
using SketchModeller.Utilities;
using SketchModeller.Infrastructure.Data.EditConstraints;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SnappedBgcConverter : BaseSnappedConverter<SnappedBendedGenCylinder>
    {
        protected override NewPrimitive ConvertCore(SnappedBendedGenCylinder snapped)
        {
            var result = new NewBendedGenCylinder();
            result.Axis.Value = snapped.AxisResult;
            result.Length.Value = snapped.LengthResult;
            result.Center.Value = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5);
            result.Components = snapped.ComponentResults.CloneArray();

            result.EditConstraints.Add(new AxisOnLineConstraint(
                snapped.BottomCenterResult,
                snapped.AxisResult,
                result.Center,
                result.Axis));
            return result;
        }
    }
}
