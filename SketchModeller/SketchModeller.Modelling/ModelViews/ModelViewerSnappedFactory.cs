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
        public static readonly Brush FRONT_BRUSH = Brushes.SkyBlue;
        public static readonly Brush FRONT_BRUSH_MARKED = Brushes.LightSkyBlue;
        public static readonly Brush BACK_BRUSH = Brushes.Red;
        public static readonly Brush BACK_SELECTED_BRUSH = Brushes.Orange;

        public Visual3D Create(object item)
        {
            Visual3D result = new ModelVisual3D();
            item.MatchClass<SnappedCylinder>(cylinderData => result = CreateCylinderView(cylinderData));
            return result;
        }

        private static ModelVisual3D CreateVisual(MeshGeometry3D geometry, SnappedPrimitive snappedPrimitive)
        {
            var frontMaterial = new DiffuseMaterial();
            frontMaterial.Bind(
                DiffuseMaterial.BrushProperty, 
                () => snappedPrimitive.IsMarked, 
                flag => flag ? FRONT_BRUSH_MARKED : FRONT_BRUSH);

            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D(
                geometry,
                frontMaterial);

            var backMaterial = new DiffuseMaterial();
            backMaterial.Bind(
                DiffuseMaterial.BrushProperty,
                () => snappedPrimitive.IsMarked,
                flag => flag ? BACK_SELECTED_BRUSH : BACK_BRUSH);
            model3d.BackMaterial = backMaterial;

            var visual = new ModelVisual3D();
            visual.Content = model3d;
            return visual;
        }
    }
}
