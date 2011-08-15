using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SphereSphereConverter : BaseNewConverter<NewSphere, NewSphere>
    {
        protected override NewSphere ConvertCore(NewSphere source, Vector3D moveVector)
        {
            var result = new NewSphere();
            result.Center.Value = source.Center.Value + moveVector;
            result.Radius.Value = source.Radius;
            return result;
        }

        protected override void ApplyMovementCore(NewSphere source, NewSphere target, Vector3D moveVector)
        {
            target.Radius.Value = source.Radius;
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
