using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SnappedSphereConverter : BaseSnappedConverter<SnappedSphere>
    {
        protected override NewPrimitive ConvertCore(SnappedSphere snapped)
        {
            return new NewSphere
            {
                Center = snapped.CenterResult,
                Radius = snapped.RadiusResult,
            };
        }
    }
}
