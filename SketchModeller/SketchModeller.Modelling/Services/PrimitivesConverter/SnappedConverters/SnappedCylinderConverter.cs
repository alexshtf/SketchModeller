using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SnappedCylinderConverter : BaseSnappedConverter<SnappedCylinder>
    {
        protected override NewPrimitive ConvertCore(SnappedCylinder snapped)
        {
            return new NewCylinder
            {
                Axis = snapped.AxisResult,
                Length = snapped.LengthResult,
                Diameter = 2 * snapped.RadiusResult,
                Center = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5),
            };
        }
    }
}
