using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class CylinderConeConverter : BaseNewConverter<NewCylinder, NewCone>
    {
        protected override NewCone ConvertCore(NewCylinder source, Vector3D moveVector)
        {
            return new NewCone
            {
                Axis = source.Axis,
                Center = source.Center + moveVector,
                Length = source.Length,
                TopRadius = source.Radius,
                BottomRadius = source.Radius,
            };
        }

        protected override void ApplyMovementCore(NewCylinder source, NewCone target, Vector3D moveVector)
        {
            target.Center = source.Center + moveVector;
        }
    }
}
