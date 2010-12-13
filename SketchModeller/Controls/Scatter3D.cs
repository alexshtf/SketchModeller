using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Petzold.Media3D;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Diagnostics.Contracts;

namespace Controls
{
    public class Scatter3D : ModelVisual3D
    {
        public Scatter3D()
        {
            Content = new GeometryModel3D();
        }

        #region Points property

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(Point3DCollection), typeof(Scatter));

        public Point3DCollection Points
        {
            get { return (Point3DCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        #endregion

        #region PointSize property

        public static readonly DependencyProperty PointSizeProperty =
            DependencyProperty.Register("PointSize", typeof(double), typeof(Scatter3D), new PropertyMetadata(0.01), ValidatePointSize);

        public double PointSize
        {
            get { return (double)GetValue(PointSizeProperty); }
            set { SetValue(PointSizeProperty, value); }
        }

        private static bool ValidatePointSize(object obj)
        {
            double value = (double)obj;
            return value > 0;
        }

        #endregion

        #region Brush property

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(Brush), typeof(Scatter3D), new PropertyMetadata(Brushes.Navy, OnBrushChanged));

        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty);}
            set { SetValue(BrushProperty, value);}
        }

        private static void OnBrushChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scatter = (Scatter3D)sender;
            scatter.OnBrushChanged();
        }

        #endregion

        private void OnBrushChanged()
        {
            var model = (GeometryModel3D)Content;
            var material = new EmissiveMaterial { Brush = this.Brush };
            material.Freeze();
            model.Material = material;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == PointsProperty || e.Property == PointSizeProperty)
            {
                var model = (GeometryModel3D)Content;

                var positions = new Point3DCollection();
                var triangleIndices = new Int32Collection();

                if (Points != null)
                {
                    foreach (var pnt in Points)
                        GenerateBox(pnt, positions, triangleIndices);

                    var geometry = new MeshGeometry3D
                    {
                        Positions = positions,
                        TriangleIndices = triangleIndices
                    };
                    geometry.Freeze();
                    model.Geometry = geometry;
                }
                else
                    model.Geometry = null;
            }
        }

        #region BOX_TRIANGLE_INDICES constant

        private static readonly int[] BOX_TRIANGLE_INDICES = 
            {
                0, 3, 2,
                1, 2, 5,
                2, 6, 5,
                3, 6, 2,
                3, 7, 6,
                0, 4, 3,
                3, 4, 7,
                4, 6, 7,
                4, 5, 6,
                0, 5, 4,
                0, 1, 5,
            };

        #endregion

        private void GenerateBox(Point3D center, Point3DCollection positions, Int32Collection triangleIndices)
        {
            const int BOX_POINTS_COUNT = 8;

            Contract.Requires(positions != null);
            Contract.Requires(triangleIndices != null);
            Contract.Ensures(positions.Count - Contract.OldValue(positions.Count) == BOX_POINTS_COUNT);
            Contract.Ensures(triangleIndices.Count - Contract.OldValue(triangleIndices.Count) == BOX_TRIANGLE_INDICES.Length);

            double x0 = center.X;
            double y0 = center.Y;
            double z0 = center.Z;

            var baseIndex = positions.Count;
            
            positions.Add(new Point3D(x0 - PointSize / 2, y0 + PointSize / 2, z0 + PointSize / 2));
            positions.Add(new Point3D(x0 + PointSize / 2, y0 + PointSize / 2, z0 + PointSize / 2));
            positions.Add(new Point3D(x0 + PointSize / 2, y0 - PointSize / 2, z0 + PointSize / 2));
            positions.Add(new Point3D(x0 - PointSize / 2, y0 - PointSize / 2, z0 + PointSize / 2));
            
            positions.Add(new Point3D(x0 - PointSize / 2, y0 + PointSize / 2, z0 - PointSize / 2));
            positions.Add(new Point3D(x0 + PointSize / 2, y0 + PointSize / 2, z0 - PointSize / 2));
            positions.Add(new Point3D(x0 + PointSize / 2, y0 - PointSize / 2, z0 - PointSize / 2));
            positions.Add(new Point3D(x0 - PointSize / 2, y0 - PointSize / 2, z0 - PointSize / 2));

            for (int i = 0; i < BOX_TRIANGLE_INDICES.Length; ++i)
                triangleIndices.Add(baseIndex + BOX_TRIANGLE_INDICES[i]);
        }
    }
}
