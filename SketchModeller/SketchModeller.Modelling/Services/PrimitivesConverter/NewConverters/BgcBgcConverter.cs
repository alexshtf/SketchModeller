using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class BgcBgcConverter : BaseNewConverter<NewBendedGenCylinder, NewBendedGenCylinder>
    {
        protected override NewBendedGenCylinder ConvertCore(NewBendedGenCylinder source, Vector3D moveVector)
        {
            var result = new NewBendedGenCylinder();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center;
            result.Length.Value = source.Length;
            result.Components = source.Components.CloneArray();
            return result;
        }

        protected override void ApplyMovementCore(NewBendedGenCylinder source, NewBendedGenCylinder target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}
