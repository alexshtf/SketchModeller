using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Utils;
using Petzold.Media3D;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Prism.Logging;
using System.Diagnostics;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for NewCylinderView.xaml
    /// </summary>
    public partial class NewCylinderView : INewPrimitiveView, IWeakEventListener
    {
        private bool isMoving;
        private Viewport3D viewport;
        private Point3D lastSketchPlanePoint;

        private NewCylinderViewModel viewModel;
        private HollowCylinderMesh cylinderMesh;
        private ILoggerFacade logger;

        public static readonly Brush UnfocusedBrush = Brushes.White;
        public static readonly Brush FocusedBrush = Brushes.LightBlue;

        public NewCylinderView()
        {
            InitializeComponent();
            cylinderMesh = new HollowCylinderMesh();
            logger = new EmptyLogger();
        }

        public NewCylinderView(NewCylinderViewModel viewModel, ILoggerFacade logger)
            : this()
        {
            this.viewModel = viewModel;
            this.logger = logger;

            viewModel.AddListener(this, () => viewModel.Center);
            viewModel.AddListener(this, () => viewModel.Diameter);
            viewModel.AddListener(this, () => viewModel.Length);
            viewModel.AddListener(this, () => viewModel.Axis);

            cylinderMesh.Radius = viewModel.Diameter * 0.5;
            cylinderMesh.Length = viewModel.Length;
            geometry.Geometry = cylinderMesh.Geometry;
            UpdateTranslation();
            UpdateRotation();

        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
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

        private void OnMouseMove(object sender, MouseEventArgs e)
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

                    viewModel.Center = viewModel.Center + moveVector;
                }
                else
                    logger.Log("Ray calc failed. Will not move the object", Category.Warn, Priority.None);
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

        private void UpdateMeshGeometry()
        {
            var geometry = cylinderMesh.Geometry.Clone() as MeshGeometry3D;
            Contract.Assume(geometry != null, "Geometry must be a MeshGeometry3D");
        }

        private void UpdateCylinderRadius()
        {
            cylinderMesh.Radius = viewModel.Diameter * 0.5;
            geometry.Geometry = cylinderMesh.Geometry;
        }

        private void UpdateCylinderLength()
        {
            cylinderMesh.Length = viewModel.Length;
            geometry.Geometry = cylinderMesh.Geometry;
        }

        private void UpdateTranslation()
        {
            translation.OffsetX = viewModel.Center.X;
            translation.OffsetY = viewModel.Center.Y;
            translation.OffsetZ = viewModel.Center.Z;
        }

        private void UpdateRotation()
        {
            var rotationAxis = Vector3D.CrossProduct(MathUtils3D.UnitY, viewModel.Axis);
            var angle = Vector3D.AngleBetween(MathUtils3D.UnitY, viewModel.Axis);
            rotation.Axis = rotationAxis;
            rotation.Angle = angle;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType != typeof(PropertyChangedEventManager))
                return false;

            var eventArgs = (PropertyChangedEventArgs)e;

            eventArgs.Match(() => viewModel.Center, UpdateTranslation);
            eventArgs.Match(() => viewModel.Length, UpdateCylinderLength);
            eventArgs.Match(() => viewModel.Axis, UpdateRotation);
            eventArgs.Match(() => viewModel.Diameter, UpdateCylinderRadius);

            return true;
        }

        NewPrimitiveViewModel INewPrimitiveView.ViewModel
        {
            get { return viewModel; }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add)
                viewModel.Edit(+1);
            if (e.Key == Key.Subtract)
                viewModel.Edit(-1);
        }

        private void OnIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var inputElement = sender as IInputElement;
            if (inputElement != null)
            {
                if (inputElement.IsKeyboardFocusWithin)
                    frontDiffuseMaterial.Brush = FocusedBrush;
                else
                    frontDiffuseMaterial.Brush = UnfocusedBrush;
            }
        }
    }
}
