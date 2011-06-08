using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using Utils;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SnappedSgcConverter : BaseSnappedConverter<SnappedStraightGenCylinder>
    {
        protected override NewPrimitive ConvertCore(SnappedStraightGenCylinder snapped)
        {
            return new NewStraightGenCylinder
            {
                Axis = snapped.AxisResult,
                Length = snapped.LengthResult,
                Center = MathUtils3D.Lerp(snapped.TopCenterResult, snapped.BottomCenterResult, 0.5),
                Components = snapped.ComponentResults.CloneArray(),
            };
        }
    }
}
