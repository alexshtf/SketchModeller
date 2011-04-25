using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using HelixToolkit;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory
    {
        public static Visual3D CreateSphereView(SnappedSphere sphereData)
        {
            Contract.Requires(sphereData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);

            var meshBuilder = new MeshBuilder(true, true);
            meshBuilder.AddSphere(sphereData.CenterResult, sphereData.RadiusResult);
            var geometry = meshBuilder.ToMesh();

            return CreateVisual(geometry, sphereData);
        }
    }
}
