using System.Linq;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.PrimitivesConverter
{
    class BgcConeConverter : BaseNewConverter<NewBendedGenCylinder, NewCone>
    {
        protected override NewCone ConvertCore(NewBendedGenCylinder source, Vector3D moveVector)
        {
            var result = new NewCone();
            result.Axis.Value = source.Axis;
            result.Center.Value = source.Center.Value + moveVector;
            result.TopRadius.Value = source.Components.Last().Radius;
            result.BottomRadius.Value = source.Components.First().Radius;
            result.Length.Value = source.Length;
            return result;
        }

        protected override void ApplyMovementCore(NewBendedGenCylinder source, NewCone target, Vector3D moveVector)
        {
            target.Center.Value = source.Center.Value + moveVector;
        }
    }
}