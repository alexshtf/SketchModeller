using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controls;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory : IVisual3DFactory
    {
        public static readonly ModelViewerSnappedFactory Instance = new ModelViewerSnappedFactory();
        public static readonly Brush FRONT_MATERIAL = Brushes.LightBlue;
        public static readonly Brush BACK_MATRIAL = Brushes.Orange;

        public Visual3D Create(object item)
        {
            Visual3D result = new ModelVisual3D();
            item.MatchClass<SnappedCylinder>(cylinderData => result = CreateCylinderView(cylinderData));
            return result;
        }

        private static ModelVisual3D CreateVisual(MeshGeometry3D geometry)
        {
            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D(
                geometry,
                new DiffuseMaterial { Brush = FRONT_MATERIAL });
            model3d.BackMaterial = new DiffuseMaterial { Brush = Brushes.Red };

            var visual = new ModelVisual3D();
            visual.Content = model3d;
            return visual;
        }
    }
}
