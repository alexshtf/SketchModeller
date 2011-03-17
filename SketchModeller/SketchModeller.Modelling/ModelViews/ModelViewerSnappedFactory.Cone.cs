using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.ModelViews
{
    public partial class ModelViewerSnappedFactory
    {
        public static Visual3D CreateConeView(SnappedCone coneData)
        {
            Contract.Requires(coneData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);

            var topCircle = ShapeHelper.GenerateCircle(
                coneData.TopFeatureCurve.CenterResult,
                coneData.TopFeatureCurve.NormalResult,
                coneData.TopFeatureCurve.RadiusResult,
                50);
            var botCircle = ShapeHelper.GenerateCircle(
                coneData.BottomFeatureCurve.CenterResult,
                coneData.BottomFeatureCurve.NormalResult,
                coneData.BottomFeatureCurve.RadiusResult,
                50);

            return CreateCylinderView(topCircle, botCircle, coneData);
        }
    }
}
