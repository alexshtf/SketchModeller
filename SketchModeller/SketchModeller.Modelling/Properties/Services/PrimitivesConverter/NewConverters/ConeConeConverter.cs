using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class ConeConeConverter : BaseNewConverter<NewCone, NewCone>
    {
        protected override NewCone ConvertCore(NewCone source, Vector3D moveVector)
        {
            return new NewCone
            {
                Axis = source.Axis,
                Center = source.Center + moveVector,
                TopRadius = source.TopRadius,
                BottomRadius = source.BottomRadius,
                Length = source.Length,
            };
        }

        protected override void ApplyMovementCore(NewCone source, NewCone target, Vector3D moveVector)
        {
            target.Center = source.Center + moveVector;
        }
    }
}
