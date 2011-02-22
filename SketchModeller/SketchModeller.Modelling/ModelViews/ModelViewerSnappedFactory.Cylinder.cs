using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Diagnostics.Contracts;
using Utils;
using SketchModeller.Utilities;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {
        private Visual3D CreateCylinderView(SnappedCylinder cylinderData)
        {
            Contract.Requires(cylinderData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);

            if (cylinderData.TopCircle == null ||
                cylinderData.BottomCircle == null)
            {
                return new ModelVisual3D();
            }
            else
                return CreateCylinderView(cylinderData.TopCircle, cylinderData.BottomCircle, cylinderData);
        }
    }
}
