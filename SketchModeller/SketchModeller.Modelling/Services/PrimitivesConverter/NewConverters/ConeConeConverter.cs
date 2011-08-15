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
            var result = new NewCone();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center.Value + moveVector;
            result.TopRadius.Value = source.TopRadius;
            result.BottomRadius.Value = source.BottomRadius;
            result.Length.Value = source.Length;
            return result;
        }

        protected override void ApplyMovementCore(NewCone source, NewCone target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
