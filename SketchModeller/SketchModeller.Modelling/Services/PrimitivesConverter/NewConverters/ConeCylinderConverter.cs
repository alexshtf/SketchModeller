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
            return new NewCylinder
            {
                Axis = source.Axis,
                Center = source.Center + moveVector,
                Diameter = source.TopRadius + source.BottomRadius,
                Length = source.Length,
            };
        }
    }
}
