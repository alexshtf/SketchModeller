using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class CylinderCylinderConverter : BaseNewConverter<NewCylinder, NewCylinder>
    {
        protected override NewCylinder ConvertCore(NewCylinder source, Vector3D moveVector)
        {
            var result = new NewCylinder();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center.Value + moveVector;
            result.Diameter.Value = source.Diameter;
            result.Length.Value = source.Length;
            return result;
        }

        protected override void ApplyMovementCore(NewCylinder source, NewCylinder target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
