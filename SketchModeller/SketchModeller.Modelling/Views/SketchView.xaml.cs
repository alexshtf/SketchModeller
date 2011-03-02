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
using Microsoft.Practices.Unity;
using Petzold.Media3D;
using System.ComponentModel;

using Utils;
using System.Windows.Media.Media3D;
using System.Reflection;
using System.Diagnostics;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Logging;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchView.xaml
    /// </summary>
    public partial class SketchView : UserControl
    {
        private static readonly Cursor ADD_CURSOR;
        private static readonly Cursor REMOVE_CURSOR;

        static SketchView()
        {
            var assembly = Assembly.GetCallingAssembly();
            using (var stream = assembly.GetManifestResourceStream("SketchModeller.Modelling.arrowadd.cur"))
            {
                ADD_CURSOR = new Cursor(stream);
            }
            using (var stream = assembly.GetManifestResourceStream("SketchModeller.Modelling.arrowdel.cur"))
            {
                REMOVE_CURSOR = new Cursor(stream);
            }
        }

        private readonly ILoggerFacade logger;
        private readonly SketchViewModel viewModel;
        private MouseInterationModes mouseInteractionMode;
        private MousePosInfo3D dragStartLocation;
        bool isDragging;

        private readonly SketchModellingView sketchModellingView;
        private readonly SketchImageView sketchImageView;

        public SketchView()
        {
            InitializeComponent();
            mouseInteractionMode = MouseInterationModes.CurveSelection;
        }

        [InjectionConstructor]
        public SketchView(SketchViewModel viewModel, IUnityContainer container, ILoggerFacade logger = null)
            : this()
        {
            this.logger = logger ?? new EmptyLogger();

            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            this.viewModel = viewModel;

            sketchModellingView = 
                container.Resolve<SketchModellingView>(
                    new DependencyOverride<SketchModellingViewModel>(viewModel.SketchModellingViewModel));
            root3d.Children.Add(sketchModellingView);

            sketchImageView =
                container.Resolve<SketchImageView>(
                    new DependencyOverride<SketchImageViewModel>(viewModel.SketchImageViewModel));
            sketchImageView.Margin = vpRoot.Margin;
            root.Children.Insert(1, sketchImageView);
        }


        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Match(() => viewModel.SketchPlane))
            {
                var sketchPlane = viewModel.SketchPlane;

                var normal = sketchPlane.Normal.Normalized();
                var center = sketchPlane.Center;
                var position = sketchPlane.Center - 50 * normal;

                var lookAt = MathUtils3D.LookAt(position, normal, sketchPlane.YAxis.Normalized());
                camera.ViewMatrix = lookAt;

                var projMatrix = Matrix3D.Identity;
                projMatrix.M33 = 0.0001;
                //projMatrix.OffsetZ = -lookAt.OffsetZ + 0.5;
                projMatrix.OffsetZ = 0.2;
                camera.ProjectionMatrix = projMatrix;
            }
        }

        private void StackPanel_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source == curveSelection)
                mouseInteractionMode = MouseInterationModes.CurveSelection;
            else if (e.Source == primitiveManipulation)
                mouseInteractionMode = MouseInterationModes.PrimitiveManipulation;
            else
                logger.Log("This should not happen", Category.Exception, Priority.High);
        }

        #region Selection + primitive events

        private void vpRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDragging && e.ChangedButton == MouseButton.Left)
            {
                dragStartLocation = GetPosition3D(e);
                vpRoot.CaptureMouse();
                isDragging = true;

                if (mouseInteractionMode == MouseInterationModes.PrimitiveManipulation)
                    SelectPrimitive(dragStartLocation);
            }
        }

        private void vpRoot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && e.ChangedButton == MouseButton.Left)
            {
                if (mouseInteractionMode == MouseInterationModes.CurveSelection)
                {
                    SelectCurves(GetPosition3D(e));
                    selectionRectangle.Visibility = Visibility.Collapsed;
                }
                else
                    StopPrimitiveDragging();
                vpRoot.ReleaseMouseCapture();
                isDragging = false;
            }
        }

        private void vpRoot_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var position = GetPosition3D(e);
                if (mouseInteractionMode == MouseInterationModes.CurveSelection)
                    UpdateSelectionRectangle(position.Pos2D);
                else if (mouseInteractionMode == MouseInterationModes.PrimitiveManipulation)
                    DragPrimitive(position);
            }
        }

        private void SelectCurves(MousePosInfo3D positionInfo)
        {
            var rect = new Rect(positionInfo.Pos2D, dragStartLocation.Pos2D);
            sketchImageView.SelectCurves(rect);
        }

        private void DragPrimitive(MousePosInfo3D positionInfo)
        {
            if (positionInfo.Ray3D != null)
                sketchModellingView.DragPrimitive(positionInfo.Ray3D.Value);
        }

        private void StopPrimitiveDragging()
        {
            sketchModellingView.EndDrag();
        }

        private void UpdateSelectionRectangle(Point point)
        {
            var rect = new Rect(point, dragStartLocation.Pos2D);
            selectionRectangle.Width = rect.Width;
            selectionRectangle.Height = rect.Height;
            Canvas.SetTop(selectionRectangle, rect.Top);
            Canvas.SetLeft(selectionRectangle, rect.Left);
            selectionRectangle.Visibility = Visibility.Visible;
        }

        private void SelectPrimitive(MousePosInfo3D positionInfo)
        {
            if (positionInfo.Ray3D != null)
                sketchModellingView.SelectPrimitive(positionInfo.Ray3D.Value);
        }

        #endregion

        private MousePosInfo3D GetPosition3D(MouseEventArgs e)
        {
            var pos2d = e.GetPosition(viewport3d);
            return GetPosition3D(pos2d);
        }

        private MousePosInfo3D GetPosition3D(Point pos2d)
        {
            LineRange lineRange;
            if (ViewportInfo.Point2DtoPoint3D(viewport3d, pos2d, out lineRange))
                return new MousePosInfo3D { Pos2D = pos2d, Ray3D = lineRange };
            else
                return new MousePosInfo3D { Pos2D = pos2d, Ray3D = null };
        }

        private void OnThumbDragStarted(object sender, RoutedEventArgs e)
        {
            PrimitiveKinds primitiveKind = default(PrimitiveKinds);
            if (sender == cylinderThumb)
                primitiveKind = PrimitiveKinds.Cylinder;
            else if (sender == coneThumb)
                primitiveKind = PrimitiveKinds.Cone;
            else
                logger.Log("Invalid event sender", Category.Exception, Priority.High);

            var dataObject = new DataObject(DataFormats.Serializable, primitiveKind);
            DragDrop.DoDragDrop((DependencyObject)sender, dataObject, DragDropEffects.Copy);
        }

        private void vpRoot_Drop(object sender, DragEventArgs e)
        {
            var mousePos2d = e.GetPosition(viewport3d);
            var pos3d = GetPosition3D(mousePos2d);

            if (pos3d.Ray3D != null)
            {
                var primitiveKind = (PrimitiveKinds)e.Data.GetData(DataFormats.Serializable, true);
                viewModel.AddNewPrimitive(primitiveKind, pos3d.Ray3D.Value);
            }
        }

        private struct MousePosInfo3D
        {
            public Point Pos2D;
            public LineRange? Ray3D;
        }

        private enum MouseInterationModes
        {
            CurveSelection,
            PrimitiveManipulation,
        }
    }
}
