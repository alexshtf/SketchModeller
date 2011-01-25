using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Microsoft.Practices.Prism.Logging;
using Controls;
using Utils;
using System.Windows.Input;
using SketchModeller.Infrastructure;
using System.Windows.Controls;
using Petzold.Media3D;
using System.Diagnostics.Contracts;
using System.Windows;

namespace SketchModeller.Modelling.Views
{
    public abstract class BaseNewPrimitiveView : ModelVisual3D, INewPrimitiveView
    {
        // general fields
        private readonly NewPrimitiveViewModel viewModel;
        private readonly ILoggerFacade logger;

        // sub-tree elements
        private readonly ModelUIElement3D uiElement;
        private readonly GeometryModel3D geometryModel;
        private readonly AxisAngleRotation3D rotation;
        private readonly TranslateTransform3D translation;
        private readonly DiffuseMaterial frontDiffuseMaterial;

        // object manipulation state
        private bool isMoving;
        private Point3D lastSketchPlanePoint;
        private Viewport3D viewport;

        public static readonly Brush UnfocusedBrush = Brushes.White;
        public static readonly Brush FocusedBrush = Brushes.LightBlue;

        public BaseNewPrimitiveView(NewPrimitiveViewModel viewModel, ILoggerFacade logger)
        {
            this.viewModel = viewModel;
            this.logger = logger;

            Children.Add(uiElement = new ModelUIElement3D());

            uiElement.Focusable = true;
            uiElement.MouseDown += new System.Windows.Input.MouseButtonEventHandler(OnMouseDown);
            uiElement.MouseUp += new System.Windows.Input.MouseButtonEventHandler(OnMouseUp);
            uiElement.MouseMove += new System.Windows.Input.MouseEventHandler(OnMouseMove);
            uiElement.KeyDown += new KeyEventHandler(OnKeyDown);
            uiElement.IsKeyboardFocusWithinChanged += new System.Windows.DependencyPropertyChangedEventHandler(OnIsKeyboardFocusWithinChanged);
            uiElement.Model = (geometryModel = new GeometryModel3D());

            geometryModel.Transform = new Transform3DGroup
            {
                Children =
                {
                    new RotateTransform3D((rotation = new AxisAngleRotation3D())),
                    (translation = new TranslateTransform3D()),
                },
            };

            geometryModel.Material = frontDiffuseMaterial = new DiffuseMaterial
            {
                Brush = UnfocusedBrush,
                Color = TransparentColorExtension.MakeTransparent(Colors.Gray, 128),
            };

            geometryModel.BackMaterial = new DiffuseMaterial
            {
                Brush = Brushes.Red,
                Color = TransparentColorExtension.MakeTransparent(Colors.Gray, 128),
            };
        }

        protected abstract void MovePosition(Vector3D moveVector);

        protected abstract void Edit(int sign);

        protected void UpdateTranslation(Point3D pos)
        {
            translation.OffsetX = pos.X;
            translation.OffsetY = pos.Y;
            translation.OffsetZ = pos.Z;
        }

        protected void UpdateRotation(Vector3D axis, double degrees)
        {
            rotation.Axis = axis;
            rotation.Angle = degrees;
        }

        protected void UpdateGeometry(Geometry3D geometry)
        {
            geometryModel.Geometry = geometry;
        }

        #region Keyboard handling

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add)
                Edit(+1);
            if (e.Key == Key.Subtract)
                Edit(-1);
        }

        private void OnIsKeyboardFocusWithinChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if (inputElement != null)
            {
                if (inputElement.IsKeyboardFocusWithin)
                    frontDiffuseMaterial.Brush = FocusedBrush;
                else
                    frontDiffuseMaterial.Brush = UnfocusedBrush;

                viewModel.Model.IsSelected = inputElement.IsKeyboardFocusWithin;
            }
        }

        #endregion

        #region Mouse handling

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isMoving)
            {
                var position = Mouse.GetPosition(uiElement);
                var maybePnt = GetPointOnSketchPlane(position);
                if (maybePnt != null)
                {
                    var pnt = maybePnt.Value;
                    var moveVector = pnt - lastSketchPlanePoint;
                    lastSketchPlanePoint = pnt;

                    MovePosition(moveVector);
                }
                else
                    logger.Log("Ray calc failed. Will not move the object", Category.Warn, Priority.None);
            }
        }


        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isMoving = false;
                logger.Log("Releasing mouse capture", Category.Debug, Priority.None);
                uiElement.ReleaseMouseCapture();
            }
            if (e.ChangedButton == MouseButton.Right)
            {
                var newEventArgs = new MenuCommandsEventArgs(RoutedEvents.ContextMenuCommandsEvent);
                newEventArgs.MenuCommands.AddRange(viewModel.ContextMenu);
                uiElement.RaiseEvent(newEventArgs);

                e.Handled = true;
            }
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isMoving = true;
                logger.Log("Capturing mouse", Category.Debug, Priority.None);
                bool success = uiElement.CaptureMouse();
                if (!success)
                    logger.Log("Mouse capture failed", Category.Warn, Priority.None);

                viewport = this.VisualPathUp().OfType<Viewport3D>().FirstOrDefault();
                var sketchPlanePoint = GetPointOnSketchPlane(e.GetPosition(uiElement));
                if (sketchPlanePoint != null)
                    lastSketchPlanePoint = sketchPlanePoint.Value;
                else
                    logger.Log("Error getting point on sketch plane", Category.Warn, Priority.None);

                logger.Log("Focusing the UIElement3D", Category.Debug, Priority.None);
                success = uiElement.Focus();
                if (!success)
                    logger.Log("Focus request failed", Category.Warn, Priority.None);
            }
        }

        private Point3D? GetPointOnSketchPlane(Point position)
        {
            LineRange ray;
            if (ViewportInfo.Point2DtoPoint3D(viewport, position, out ray))
            {
                var plane3d = Plane3D.FromPointAndNormal(viewModel.SketchPlane.Center, viewModel.SketchPlane.Normal);
                var t = plane3d.IntersectLine(ray.Point1, ray.Point2);
                Contract.Assume(t > 0, "The sketch-plane is before us. The ray always intersects it");

                var intersectionPoint = MathUtils3D.Lerp(ray.Point1, ray.Point2, t);
                return intersectionPoint;
            }
            else
                return null;
        }

        #endregion

        NewPrimitiveViewModel INewPrimitiveView.ViewModel
        {
            get { return viewModel; }
        }
    }
}
