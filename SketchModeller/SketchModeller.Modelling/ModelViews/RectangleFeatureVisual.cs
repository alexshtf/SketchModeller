using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using Utils;
using System.Windows.Media;
using HelixToolkit;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling.ModelViews
{
    class RectangleFeatureVisual : ModelVisual3D, IFeatureCurveVisual
    {
        private readonly RectangleFeatureCurve featureCurve;
        private readonly GeometryModel3D model3d;
        private readonly double thickness;

        public RectangleFeatureVisual(RectangleFeatureCurve featureCurve, double thickness = 0.01)
        {
            Contract.Requires(thickness > 0);
            Contract.Requires(featureCurve != null);
            Contract.Ensures(object.ReferenceEquals(featureCurve, FeatureCurve));

            this.featureCurve = featureCurve;
            this.thickness = thickness;

            var emissiveMaterial = new EmissiveMaterial();
            emissiveMaterial.Bind(
                EmissiveMaterial.BrushProperty,
                () => featureCurve.IsSelected,
                isSelected => isSelected ? Brushes.DarkOrange : Brushes.Black);

            var material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial { Brush = Brushes.Black });
            material.Children.Add(emissiveMaterial);

            model3d = new GeometryModel3D();
            model3d.Material = model3d.BackMaterial = material;
            Content = model3d;

            Update();
        }

        public FeatureCurve FeatureCurve
        {
            get { return featureCurve; }
        }

        public void Update()
        {
            model3d.Geometry = GenerateMeshGeometry(featureCurve.CenterResult, 
                                                    featureCurve.NormalResult, 
                                                    featureCurve.WidthVectorResult, 
                                                    featureCurve.WidthResult - 0.03, 
                                                    featureCurve.HeightResult - 0.03);
        }

        private Geometry3D GenerateMeshGeometry(Point3D center, Vector3D normal, Vector3D widthVector, double width, double height)
        {
            var points = ShapeHelper.GenerateRectangle(center, normal, widthVector, width, height);
            MeshBuilder builder = new MeshBuilder();
            builder.AddTube(points, thickness, 10, isTubeClosed: true);
            return builder.ToMesh();
        }
    }
}
