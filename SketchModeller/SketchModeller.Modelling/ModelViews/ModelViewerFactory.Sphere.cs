using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using Petzold.Media3D;
using Utils;
using System.Windows.Media;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerFactory
    {
        private Visual3D CreateSphereView(NewSphere sphereData)
        {
            Contract.Requires(sphereData != null);
            Contract.Ensures(Contract.Result<Visual3D>() != null);

            var sphere = new Sphere();
            sphere.Bind(Sphere.CenterProperty, () => sphereData.Center, center => center.Value);
            sphere.Bind(Sphere.RadiusProperty, () => sphereData.Radius, radius => radius.Value);

            sphere.Material = new DiffuseMaterial { Brush = Brushes.White };

            return sphere;
        }
    }
}
