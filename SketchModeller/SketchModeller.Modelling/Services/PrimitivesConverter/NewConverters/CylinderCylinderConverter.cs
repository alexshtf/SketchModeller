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
            return new NewCylinder
            {
                Axis = source.Axis,
                Center = source.Center + moveVector,
                Diameter = source.Diameter,
                Length = source.Length,
            };
        }
    }
}
