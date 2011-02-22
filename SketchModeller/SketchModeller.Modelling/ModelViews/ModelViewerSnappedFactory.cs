using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Controls;
using System.Windows.Media.Media3D;
using Utils;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media;
using System.Diagnostics.Contracts;

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

        private static Visual3D CreateCylinderView(Point3D[] topPoints, Point3D[] botPoints, SnappedPrimitive snappedPrimitive)
        {
            Contract.Requires(topPoints.Length == botPoints.Length);
            Contract.Requires(snappedPrimitive != null);

            var m = topPoints.Length;

            // top points indices [0 .. m-1]
            var topIdx = System.Linq.Enumerable.Range(0, m).ToArray();

            // bottom points indices [m .. 2*m - 1]
            var bottomIdx = System.Linq.Enumerable.Range(m, m).ToArray();
            Contract.Assume(topIdx.Length == bottomIdx.Length);

            // create cylinder geometry
            var geometry = new MeshGeometry3D();
            geometry.Positions = new Point3DCollection(topPoints.Concat(botPoints));
            geometry.TriangleIndices = new Int32Collection();
            for (int i = 0; i < m; ++i)
            {
                var j = (i + 1) % m;
                var pc = topIdx[i];
                var pn = topIdx[j];
                var qc = bottomIdx[i];
                var qn = bottomIdx[j];

                geometry.TriangleIndices.AddMany(pc, qc, pn);
                geometry.TriangleIndices.AddMany(qc, qn, pn);
            }

            return CreateVisual(geometry, snappedPrimitive);
        }
    }
}
