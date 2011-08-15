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
            var result = new NewCone();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center.Value + moveVector;
            result.Length.Value = source.Length;
            result.TopRadius.Value = source.Radius * 0.8;
            result.BottomRadius.Value = source.Radius * 1.2;
            return result;
        }

        protected override void ApplyMovementCore(NewCylinder source, NewCone target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
