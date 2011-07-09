using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SgcConeConverter : BaseNewConverter<NewStraightGenCylinder, NewCone>
    {
        protected override NewCone ConvertCore(NewStraightGenCylinder source, Vector3D moveVector)
        {
            return new NewCone
            {
                Axis = source.Axis,
                Center = source.Center + moveVector,
                TopRadius = source.Components.Last().Radius,
                BottomRadius = source.Components.First().Radius,
                Length = source.Length,
            };
        }

        protected override void ApplyMovementCore(NewStraightGenCylinder source, NewCone target, Vector3D moveVector)
        {
            target.Center = source.Center + moveVector;
        }
    }
}
