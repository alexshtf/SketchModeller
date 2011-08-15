using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class SgcSgcConverter : BaseNewConverter<NewStraightGenCylinder, NewStraightGenCylinder>
    {
        protected override NewStraightGenCylinder ConvertCore(NewStraightGenCylinder source, Vector3D moveVector)
        {
            var result = new NewStraightGenCylinder();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center;
            result.Length.Value = source.Length;
            result.Components = source.Components.CloneArray();
            return result;
        }

        protected override void ApplyMovementCore(NewStraightGenCylinder source, NewStraightGenCylinder target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
