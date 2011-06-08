using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SgcSgcConverter : BaseNewConverter<NewStraightGenCylinder, NewStraightGenCylinder>
    {
        protected override NewStraightGenCylinder ConvertCore(NewStraightGenCylinder source, Vector3D moveVector)
        {
            return new NewStraightGenCylinder
            {
                Axis = source.Axis,
                Center = source.Center,
                Length = source.Length,
                Components = source.Components.CloneArray(),
            };
        }

        protected override void ApplyMovementCore(NewStraightGenCylinder source, NewStraightGenCylinder target, Vector3D moveVector)
        {
            target.Center = source.Center + moveVector;
        }
    }
}
