using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.ModelViews
{
    public partial class ModelViewerSnappedFactory
    {
        public static Visual3D CreateConeView(SnappedCone coneData)
        {
            Contract.Requires(coneData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);

            if (coneData.TopCircle == null || coneData.BottomCircle == null)
                return new ModelVisual3D();
            else
                return CreateCylinderView(coneData.TopCircle, coneData.BottomCircle, coneData);
        }
    }
}
