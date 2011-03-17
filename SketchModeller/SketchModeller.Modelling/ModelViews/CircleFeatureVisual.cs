using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using SketchModeller.Utilities;
using Utils;
using SketchModeller.Infrastructure.Data;
using System.Windows.Media;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.ModelViews
{
    class CircleFeatureVisual : ModelVisual3D, IFeatureCurveVisual
    {
        private const int CIRCLE_SAMPLES_COUNT = 50;
        private const int CROSS_SECTION_SAMPLES_COUNT = 10;

        private readonly CircleFeatureCurve featureCurve;
        private readonly GeometryModel3D model3d;
        private readonly double thickness;

        public CircleFeatureVisual(CircleFeatureCurve featureCurve, double thickness = 0.01)
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
                isSelected => isSelected ? Brushes.Orange : Brushes.Blue);

            var material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial { Brush = Brushes.DarkGray });
            material.Children.Add(emissiveMaterial);

            model3d = new GeometryModel3D();
            model3d.Material = model3d.BackMaterial = material;
            Content = model3d;

            Update();
        }

        public void Update()
        {
            var normal = featureCurve.NormalResult;
            var center = featureCurve.CenterResult;
            var radius = featureCurve.RadiusResult;

            var points = ShapeHelper.GenerateCircle(center, normal, radius, CIRCLE_SAMPLES_COUNT).ToList();
            points.Add(points.First());

            model3d.Geometry = GenerateMeshGeometry(points, normal);
        }

        public FeatureCurve FeatureCurve
        {
            get { return featureCurve; }
        }

        private MeshGeometry3D GenerateMeshGeometry(IList<Point3D> points, Vector3D xAxis)
        {
            // normalize the x axis in order to satisfy the requirements of ShapeHelper.GenerateCircle
            xAxis.Normalize();

            // generate all circles
            var circles = new List<IList<Point3D>>();
            var normals = GenerateNormals(points);
            for (int i = 0; i < points.Count; ++i)
            {
                var pnt = points[i];
                var nrm = normals[i];
                var yAxis = Vector3D.CrossProduct(xAxis, nrm).Normalized();
                var circle = ShapeHelper.GenerateCircle(pnt, xAxis, yAxis, thickness, CROSS_SECTION_SAMPLES_COUNT);
                circles.Add(circle);
            }

            // generate the mesh geometry
            var geometry = new MeshGeometry3D();
            geometry.Positions = new Point3DCollection(circles.SelectMany(c => c)); // put in all points
            for (int i = 0; i < circles.Count; ++i)
            {
                var currBaseIndex = i * CROSS_SECTION_SAMPLES_COUNT;
                var nextBaseIndex = currBaseIndex + CROSS_SECTION_SAMPLES_COUNT;
                for (int j = 0; j < CROSS_SECTION_SAMPLES_COUNT; ++j)
                {
                    var c1 = currBaseIndex + j;
                    var c2 = currBaseIndex + (j + 1) % CROSS_SECTION_SAMPLES_COUNT;
                    var n1 = nextBaseIndex + j;
                    var n2 = nextBaseIndex + (j + 1) % CROSS_SECTION_SAMPLES_COUNT;

                    geometry.TriangleIndices.Add(c1);
                    geometry.TriangleIndices.Add(c2);
                    geometry.TriangleIndices.Add(n1);

                    geometry.TriangleIndices.Add(n1);
                    geometry.TriangleIndices.Add(c2);
                    geometry.TriangleIndices.Add(n2);
                }
            }

            return geometry;
        }

        private IList<Vector3D> GenerateNormals(IList<Point3D> points)
        {
            // generate initial normals
            var initialNormals = new Vector3D[points.Count];
            for (int i = 0; i < points.Count - 1; ++i)
            {
                initialNormals[i] = points[i + 1] - points[i];
                initialNormals[i].Normalize();
            }
            initialNormals[initialNormals.Length - 1] = initialNormals[initialNormals.Length - 2];

            // average intermediate normals in the initial normals
            var averagedNormals = new Vector3D[points.Count];
            for (int i = 1; i < points.Count - 1; ++i)
            {
                averagedNormals[i] = initialNormals[i - 1] + initialNormals[i + 1];
                averagedNormals[i].Normalize();
            }
            averagedNormals[0] = initialNormals[0];
            averagedNormals[points.Count - 1] = initialNormals[points.Count - 1];

            return averagedNormals;
        }
    }
}
