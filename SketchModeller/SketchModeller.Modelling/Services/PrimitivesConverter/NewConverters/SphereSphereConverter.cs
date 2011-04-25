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
            return new NewSphere
            {
                Center = source.Center + moveVector,
                Radius = source.Radius,
            };
        }

        protected override void ApplyMovementCore(NewSphere source, NewSphere target, Vector3D moveVector)
        {
            target.Radius = source.Radius;
            target.Center = source.Center + moveVector;
        }
    }
}
