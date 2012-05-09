using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using Utils;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {

        public static Visual3D CreateCuboidView(SnappedCuboid cuboidData)
        {
            Contract.Requires(cuboidData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);
            var visual = new ModelVisual3D();
            visual.Children.Add(CreateCuboidView(cuboidData.CenterResult, cuboidData.WidthResult, cuboidData.HeightResult, cuboidData.DepthResult,
                                    cuboidData.Wresult, cuboidData.Hresult, cuboidData.Dresult, cuboidData));
            return visual;
        }
    }
}
