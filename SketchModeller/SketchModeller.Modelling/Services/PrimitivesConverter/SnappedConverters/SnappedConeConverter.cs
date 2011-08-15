using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SnappedConeConverter : BaseSnappedConverter<SnappedCone>
    {
        protected override NewPrimitive ConvertCore(SnappedCone snapped)
        {
            var result = new NewCone();
            result.Axis.Value = snapped.AxisResult;
            result.TopRadius.Value = snapped.TopRadiusResult;
            result.BottomRadius.Value = snapped.BottomRadiusResult;
            result.Length.Value = snapped.LengthResult;
            result.Center.Value = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5);
            return result;
        }
    }
}
