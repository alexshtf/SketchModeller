using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class ConeCylinderConverter : BaseNewConverter<NewCone, NewCylinder>
    {
        protected override NewCylinder ConvertCore(NewCone source, Vector3D moveVector)
        {
            var result = new NewCylinder();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center.Value + moveVector;
            result.Diameter.Value = source.TopRadius + source.BottomRadius;
            result.Length.Value = source.Length;
            return result;
        }

        protected override void ApplyMovementCore(NewCone source, NewCylinder target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
