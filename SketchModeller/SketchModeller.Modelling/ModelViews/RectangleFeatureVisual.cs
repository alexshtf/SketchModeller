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
            var normal = featureCurve.NormalResult;
            var center = featureCurve.CenterResult;
            
            var width = featureCurve.WidthResult;
            var height = featureCurve.HeightResult;

            var widthVector = featureCurve.WidthVectorResult;
            var heightVector = Vector3D.CrossProduct(normal, widthVector);

            widthVector.Normalize();
            heightVector.Normalize();

            model3d.Geometry = GenerateMeshGeometry(center, width, height, widthVector, heightVector);
        }

        private Geometry3D GenerateMeshGeometry(Point3D center, double width, double height, Vector3D widthVector, Vector3D heightVector)
        {
            var tl = center - width * widthVector + height * heightVector;
            var tr = center + width * widthVector + height * heightVector;
            var br = center + width * widthVector - height * heightVector;
            var bl = center - width * widthVector - height * heightVector;

            MeshBuilder builder = new MeshBuilder();
            var points = new Point3D[] { tl, tr, br, bl };
            builder.AddTube(points, thickness, 10, isTubeClosed: true);
            return builder.ToMesh();
        }
    }
}
