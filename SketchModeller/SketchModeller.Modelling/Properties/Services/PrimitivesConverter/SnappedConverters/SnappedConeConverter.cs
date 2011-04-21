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
            return new NewCone
            {
                Axis = snapped.AxisResult,
                TopRadius = snapped.TopRadiusResult,
                BottomRadius = snapped.BottomRadiusResult,
                Length = snapped.LengthResult,
                Center = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5),
            };
        }
    }
}
