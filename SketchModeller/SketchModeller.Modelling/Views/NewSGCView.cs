using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Logging;
using HelixToolkit;
using System.ComponentModel;
using Utils;
using System.Windows.Media;
using System.Windows.Input;
using Petzold.Media3D;
using System.Collections.ObjectModel;

namespace SketchModeller.Modelling.Views
{
    class NewSGCView : BaseNewPrimitiveView
    {
        private const int CIRCLE_DIV = 20;
        
        public const double MIN_LENGTH = 0.01;
        public const double MIN_DIAMETER = 0.01;

        private const ModifierKeys TRACKBALL_MODIFIERS = ModifierKeys.Alt;
        private const ModifierKeys LENGTH_MODIFIER = ModifierKeys.Control;
        private const ModifierKeys DIAMETER_MODIFIER = ModifierKeys.Shift;
        private const ModifierKeys AXIS_MOVE_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;


        private readonly NewSGCViewModel viewModel;
        private readonly ModelVisual3D modelVisual;
        private readonly GeometryModel3D model;
        
        // the idex of the component that will be edited by the drag operation.
        private int dragStartComponent; 

        public NewSGCView(NewSGCViewModel viewModel, ILoggerFacade logger)
            : base(viewModel, logger)
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += viewModel_PropertyChanged;

            model = new GeometryModel3D
            {
                Geometry = CreateGeometry(viewModel),
                Material = GetDefaultFrontMaterial(viewModel),
                BackMaterial = GetDefaultBackMaterial(),
            };
            modelVisual = new ModelVisual3D { Content = model };

            Children.Add(modelVisual);
        }

        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            model.Geometry = CreateGeometry(viewModel);
        }

        private static MeshGeometry3D CreateGeometry(NewSGCViewModel viewModel)
        {
            var startPoint = viewModel.Center - 0.5 * viewModel.Length * viewModel.Axis;
            var endPoint = viewModel.Center + 0.5 * viewModel.Length * viewModel.Axis;
            var components = viewModel.Components;

            var path = from component in components
                       select MathUtils3D.Lerp(startPoint, endPoint, component.Progress);

            var diameters = from component in components
                            select 2 * component.Radius;

            var builder = new MeshBuilder();
            builder.AddTube(
                path.ToArray(),
                null,
                diameters.ToArray(),
                thetaDiv: CIRCLE_DIV,
                isTubeClosed: false);
            var geometry = builder.ToMesh(freeze: true);
            return geometry;
        }

        public override void DragStart(Point startPos, LineRange startRay)
        {
            base.DragStart(startPos, startRay);
        }

        protected override void PerformDrag(
            Vector dragVector2d, 
            Vector3D dragVector3d, 
            Vector3D axisDragVector, 
            Point3D? sketchPlanePosition)
        {
            if (Keyboard.Modifiers == ModifierKeys.None)
                viewModel.Center = viewModel.Center + dragVector3d;
            else if (Keyboard.Modifiers == AXIS_MOVE_MODIFIER)
                viewModel.Center = viewModel.Center + axisDragVector;
            else if (Keyboard.Modifiers == TRACKBALL_MODIFIERS)
            {
                viewModel.Axis = TrackballRotate(viewModel.Axis, dragVector2d);
            }
            else if (Keyboard.Modifiers == DIAMETER_MODIFIER)
            {
                var axis = Vector3D.CrossProduct(viewModel.Axis, viewModel.SketchPlane.Normal);
                if (axis != default(Vector3D))
                {
                    axis.Normalize();
                    var radiusDelta = 0.5 * Vector3D.DotProduct(axis, dragVector3d);
                    viewModel.Components = RecomputeComponents(
                        viewModel.Components,
                        radiusDelta,
                        dragStartComponent);
                }
            }
            else if (Keyboard.Modifiers == LENGTH_MODIFIER)
            {
                var axis = viewModel.Axis.Normalized();
                var lengthDelta = Vector3D.DotProduct(axis, dragVector3d) * 2;
                viewModel.Length = Math.Max(MIN_LENGTH, viewModel.Length + lengthDelta);
            }
        }

        private ReadOnlyCollection<NewSGCViewModel.ComponentViewModel> RecomputeComponents(System.Collections.ObjectModel.ReadOnlyCollection<NewSGCViewModel.ComponentViewModel> readOnlyCollection, double radiusDelta, int dragStartComponent)
        {
            throw new NotImplementedException();
        }

        protected override Vector3D ApproximateAxis
        {
            get { return viewModel.Axis; }
        }
    }
}
