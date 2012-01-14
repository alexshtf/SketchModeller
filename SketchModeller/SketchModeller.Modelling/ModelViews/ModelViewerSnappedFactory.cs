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
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.ModelViews
{
    partial class ModelViewerSnappedFactory : IVisual3DFactory
    {
        public static readonly ModelViewerSnappedFactory Instance = new ModelViewerSnappedFactory();
        public static readonly Brush FRONT_BRUSH = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA6C1D0"));
        public static readonly Brush FRONT_BRUSH_MARKED = Brushes.LightSkyBlue;
        public static readonly Brush BACK_BRUSH = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB5AFD8"));
        public static readonly Brush BACK_SELECTED_BRUSH = Brushes.Orange;

        public Visual3D Create(object item)
        {
            Visual3D result = null;
            item.MatchClass<SnappedCylinder>(cylinderData => result = CreateCylinderView(cylinderData));
            item.MatchClass<SnappedCone>(coneData => result = CreateConeView(coneData));
            item.MatchClass<SnappedSphere>(sphereData => result = CreateSphereView(sphereData));
            item.MatchClass<SnappedStraightGenCylinder>(sgc => result = CreateSgcView(sgc));
            item.MatchClass<SnappedBendedGenCylinder>(bgc => result = CreateBgcView(bgc));

            Contract.Assume(result != null);
            PrimitivesPickService.SetPrimitiveData(result, item as SnappedPrimitive);
            return result;
        }

        private static ModelVisual3D CreateVisual(SnappedPrimitive snappedPrimitive, Action<GeometryModel3D> geometryModelAction)
        {
            var frontEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var frontDiffusePart = new DiffuseMaterial { Brush = FRONT_BRUSH };
            var frontMaterial = new MaterialGroup { Children = { frontEmissivePart, frontDiffusePart } };

            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D();
            model3d.Material = frontMaterial;

            var backEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var backDiffusePart = new DiffuseMaterial { Brush = BACK_BRUSH };
            var backMaterial = new MaterialGroup { Children = { backEmissivePart, backDiffusePart } };
            model3d.BackMaterial = backMaterial;

            var visual = new ModelVisual3D();
            visual.Content = model3d;

            geometryModelAction(model3d);

            CreateFeatureCurves(visual, snappedPrimitive.FeatureCurves);
            return visual;
        }

        private static ModelVisual3D CreateVisual(MeshGeometry3D geometry, SnappedPrimitive snappedPrimitive)
        {
            var frontEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var frontDiffusePart = new DiffuseMaterial { Brush = FRONT_BRUSH };
            var frontMaterial = new MaterialGroup { Children = { frontEmissivePart, frontDiffusePart } };

            // create wpf classes for displaying the geometry
            var model3d = new GeometryModel3D(
                geometry,
                frontMaterial);

            var backEmissivePart = CreateEmissiveMaterial(snappedPrimitive);
            var backDiffusePart = new DiffuseMaterial { Brush = BACK_BRUSH };
            var backMaterial = new MaterialGroup { Children = { backEmissivePart, backDiffusePart } };
            model3d.BackMaterial = backMaterial;

            var visual = new ModelVisual3D();
            visual.Content = model3d;

            CreateFeatureCurves(visual, snappedPrimitive.FeatureCurves);
            return visual;
        }

        private static EmissiveMaterial CreateEmissiveMaterial(SnappedPrimitive snappedPrimitive)
        {
            var frontEmissivePart = new EmissiveMaterial();
            frontEmissivePart.Bind(
                EmissiveMaterial.BrushProperty,
                () => snappedPrimitive.IsSelected,
                flag => flag ? Brushes.LightGray : Brushes.Gray);
            return frontEmissivePart;
        }

        private static void CreateFeatureCurves(ModelVisual3D root, FeatureCurve[] featureCurves)
        {
            foreach (var item in featureCurves)
            {
                ModelVisual3D featureCurveVisual = null;
                item.MatchClass<CircleFeatureCurve>(cfc => featureCurveVisual = new CircleFeatureVisual(cfc));
                Debug.Assert(featureCurveVisual != null, "Unknown feature curve type");

                root.Children.Add(featureCurveVisual);
            }
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
