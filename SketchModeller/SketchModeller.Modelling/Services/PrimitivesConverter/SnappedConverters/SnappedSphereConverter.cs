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
            var result = new NewSphere();
            result.Center.Value = snapped.CenterResult;
            result.Radius.Value = snapped.RadiusResult;
            return result;
        }
    }
}
