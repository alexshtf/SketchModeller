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
            var result = new NewCone();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center.Value + moveVector;
            result.TopRadius.Value = source.Components.Last().Radius;
            result.BottomRadius.Value = source.Components.First().Radius;
            result.Length.Value = source.Length;
            return result;
        }

        protected override void ApplyMovementCore(NewStraightGenCylinder source, NewCone target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
