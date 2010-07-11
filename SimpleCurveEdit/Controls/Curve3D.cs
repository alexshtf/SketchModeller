using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using Utils;
using System.Windows.Media;
using System.Windows.Threading;

namespace Controls
{
    public class Curve3D : ModelVisual3D
    {
        private MeshGeometry3D geometry;
        private DispatcherOperation geometryUpdateOperation;

        public Curve3D()
        {
            geometry = new MeshGeometry3D();
            var model = new GeometryModel3D();
            model.Material = new EmissiveMaterial
            {
                Brush = Brushes.White,
            };
            model.BackMaterial = new EmissiveMaterial
            {
                Brush = Brushes.White,
            };
            model.Geometry = geometry;
            Content = model;
        }

        #region Positions property

        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register("Positions", typeof(Point3DCollection), typeof(Curve3D), new PropertyMetadata(OnPositionsChanged));

        public Point3DCollection Positions
        {
            get { return (Point3DCollection)GetValue(PositionsProperty); }
            set { SetValue(PositionsProperty, value); }
        }

        private static void OnPositionsChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldPositions = e.OldValue as Point3DCollection;
            var newPositions = e.NewValue as Point3DCollection;
            var curve = sender as Curve3D;
            curve.OnPositionsChanged(oldPositions, newPositions);
        }

        #endregion

        private void OnPositionsChanged(Point3DCollection oldPositions, Point3DCollection newPositions)
        {
            if (oldPositions != null)
                oldPositions.Changed -= OnPositionsItemsChanged;

            if (newPositions != null)
                newPositions.Changed += OnPositionsItemsChanged;

            TryUpdateGeometry();
        }


        private void OnPositionsItemsChanged(object sender, EventArgs e)
        {
            TryUpdateGeometry();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            TryUpdateGeometry();
        }

        private void TryUpdateGeometry()
        {
            var shouldStart = geometryUpdateOperation == null || geometryUpdateOperation.Status != DispatcherOperationStatus.Pending;
            if (shouldStart)
                geometryUpdateOperation = Dispatcher.BeginInvoke(new Action(UpdateGeometry));
        }

        private void UpdateGeometry()
        {
            if (Positions == null)
                return;

            var viewport = this.GetViewport();
            if (viewport == null)
                return;

            var camera = viewport.Camera as ProjectionCamera;
            if (camera == null)
                return;

            var transformTo2D = TransformToAncestor(viewport);
            var points = new Point3DCollection();
            var indices = new Int32Collection();
            foreach (var pair in Positions.SeqPairs())
            {
                var prev = pair.Item1;
                var curr = pair.Item2;

                var vec = curr - prev;
                var perp = Vector3D.CrossProduct(vec, camera.LookDirection);
                perp.Normalize();

                // create scale pairs ad prev and curr points
                // item1 is scale for plus, item2 is scale for minus
                var prevScale = CalcLocalScales(prev, perp, transformTo2D);
                var currScale = CalcLocalScales(curr, perp, transformTo2D);

                var p1 = prev - prevScale.Item2 * perp;
                var p2 = prev + prevScale.Item1 * perp;
                var p3 = curr - currScale.Item2 * perp;
                var p4 = curr + currScale.Item1 * perp;

                var idx = points.Count;
                points.Add(p1); points.Add(p2); points.Add(p3); points.Add(p4);
                
                // form the triangle 
                // add first triangle
                indices.Add(idx + 2);
                indices.Add(idx + 1);
                indices.Add(idx);

                // add second triangle
                indices.Add(idx + 2);
                indices.Add(idx + 3);
                indices.Add(idx + 1);
            }

            geometry.Positions = points;
            geometry.TriangleIndices = indices;
        }

        private Tuple<double, double> CalcLocalScales(Point3D source, Vector3D direction, GeneralTransform3DTo2D transform)
        {
            var plus = source + direction;
            var minus = source - direction;

            var pnt2d = transform.Transform(source);
            var plus2d = transform.Transform(plus);
            var minus2d = transform.Transform(minus);

            var plusLength = (pnt2d - plus2d).Length;
            var minusLength = (pnt2d - minus2d).Length;

            return Tuple.Create(1 / plusLength, 1 / minusLength);
        }
    }
}
