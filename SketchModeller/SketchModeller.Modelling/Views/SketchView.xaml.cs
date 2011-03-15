using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Unity;
using Petzold.Media3D;
using SketchModeller.Infrastructure;
using SketchModeller.Infrastructure.Events;
using Utils;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Utilities;
using System.Collections.Generic;

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
                if (stream != null)
                    ADD_CURSOR = new Cursor(stream);
            }
            using (var stream = assembly.GetManifestResourceStream("SketchModeller.Modelling.arrowdel.cur"))
            {
                if (stream != null)
                    REMOVE_CURSOR = new Cursor(stream);
            }
        }

        private readonly ILoggerFacade logger;
        private readonly SketchViewModel viewModel;
        private readonly Dictionary<MouseInterationModes, IDragStrategy> dragStrategies;
        private IDragStrategy currentDragStrategy;

        private readonly SketchModellingView sketchModellingView;
        private readonly SketchImageView sketchImageView;

        public SketchView()
        {
            InitializeComponent();
            dragStrategies = new Dictionary<MouseInterationModes, IDragStrategy>();
        }

        [InjectionConstructor]
        public SketchView(SketchViewModel viewModel, UiState uiState, IEventAggregator eventAggregator, IUnityContainer container, ILoggerFacade logger = null)
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
            Grid.SetRow(sketchImageView, 1);
            sketchImageView.Margin = vpRoot.Margin;
            root.Children.Insert(1, sketchImageView);

            eventAggregator.GetEvent<GlobalShortcutEvent>().Subscribe(OnGlobalShortcut);

            dragStrategies[MouseInterationModes.CurveSelection] = 
                new CurveDragStrategy(uiState, sketchImageView, selectionRectangle);
            dragStrategies[MouseInterationModes.PrimitiveManipulation] =
                new PrimitiveDragStrategy(uiState, sketchModellingView);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Match(() => viewModel.SketchPlane))
            {
                var sketchPlane = viewModel.SketchPlane;

                var normal = sketchPlane.Normal.Normalized();
                var center = sketchPlane.Center;
                var position = sketchPlane.Center - 50 * normal;

                var lookAt = MathUtils3D.LookAt(position, normal, sketchPlane.YAxis.Normalized());
                camera.ViewMatrix = lookAt;

                light.Direction = normal;

                var projMatrix = Matrix3D.Identity;
                projMatrix.M33 = 0.0001;
                //projMatrix.OffsetZ = -lookAt.OffsetZ + 0.5;
                projMatrix.OffsetZ = 0.2;
                camera.ProjectionMatrix = projMatrix;
            }
        }

        private void OnGlobalShortcut(KeyEventArgs e)
        {
            if (e.Key == Key.Z)
                viewModel.CycleMouseInteractionMode.Execute(null);
        }

        private void vpRoot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && viewModel.MouseInteractionMode == MouseInterationModes.PrimitiveManipulation)
                viewModel.DeleteNewPrimitives();
        }

        #region Sketch viewport mouse events

        private void vpRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            vpRoot.Focus();
            if (currentDragStrategy == null && e.ChangedButton == MouseButton.Left)
            {
                currentDragStrategy = dragStrategies[viewModel.MouseInteractionMode];
                currentDragStrategy.OnMouseDown(GetPosition3D(e));
                vpRoot.CaptureMouse();
            }
        }

        private void vpRoot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDragStrategy != null && e.ChangedButton == MouseButton.Left)
            {
                currentDragStrategy.OnMouseUp(GetPosition3D(e));
                vpRoot.ReleaseMouseCapture();
                currentDragStrategy = null;
            }
        }

        private void vpRoot_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentDragStrategy != null && currentDragStrategy.IsDragging)
                currentDragStrategy.OnMouseMove(GetPosition3D(e));
        }

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

        #endregion

        #region Primitive drag & drop handling

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

        #endregion

        #region MousePosInfo3D structure
        
        private struct MousePosInfo3D
        {
            public Point Pos2D;
            public LineRange? Ray3D;
        }

        #endregion

        #region IDragStrategy interface

        [ContractClass(typeof(IDragStragegyContract))]
        private interface IDragStrategy
        {
            /// <summary>
            /// Called when user presses the mouse to start dragging.
            /// </summary>
            /// <param name="position">2D and 3D mouse position information</param>
            void OnMouseDown(MousePosInfo3D position);


            /// <summary>
            /// Called when the user drags the mouse.
            /// </summary>
            /// <param name="position">2D and 3D mouse position information</param>
            void OnMouseMove(MousePosInfo3D position);


            /// <summary>
            /// Called when the user releases the mouse and finishes the drag operation.
            /// </summary>
            /// <param name="position">2D and 3D position information</param>
            void OnMouseUp(MousePosInfo3D position);

            /// <summary>
            /// Gets a value indicating whether the user is dragging.
            /// </summary>
            /// <value>
            /// 	<c>true</c> if the user is dragging; otherwise, <c>false</c>.
            /// </value>
            bool IsDragging { get; }
        }

        [ContractClassFor(typeof(IDragStrategy))]
        private abstract class IDragStragegyContract : IDragStrategy
        {
            public void OnMouseDown(MousePosInfo3D position)
            {
                Contract.Requires(IsDragging == false);
                Contract.Ensures(IsDragging == true);
            }

            public void OnMouseMove(MousePosInfo3D position)
            {
                Contract.Requires(IsDragging == true);
            }

            public void OnMouseUp(MousePosInfo3D position)
            {
                Contract.Requires(IsDragging == true);
                Contract.Ensures(IsDragging == false);
            }

            public bool IsDragging
            {
                get { return default(bool); }
            }
        }

        #endregion

        #region DragStrategyBase class

        private abstract class DragStrategyBase : IDragStrategy
        {
            private readonly UiState uiState;
            private bool isDragging;

            public DragStrategyBase(UiState uiState)
            {
                this.uiState = uiState;
            }

            public void OnMouseDown(MousePosInfo3D position)
            {
                isDragging = true;
                LastPosition = StartPosition = position;
                MouseDownCore(position);
            }

            public void OnMouseMove(MousePosInfo3D position)
            {
                Vector vec2d;
                Vector3D? vec3d;
                GetDragVectors(position, out vec2d, out vec3d);
                MouseMoveCore(position, vec2d, vec3d);
                LastPosition = position;
            }

            public void OnMouseUp(MousePosInfo3D position)
            {
                Vector vec2d;
                Vector3D? vec3d;
                GetDragVectors(position, out vec2d, out vec3d);
                MouseUpCore(position, vec2d, vec3d);
                IsDragging = false;
            }

            public bool IsDragging
            {
                get { return isDragging; }
                private set { isDragging = value; }
            }

            /// <summary>
            /// Gets the shared UiState object
            /// </summary>
            protected UiState UiState { get { return uiState; } }

            /// <summary>
            /// Gets the mouse position info when <see cref="OnMouseDown"/> was called.
            /// </summary>
            protected MousePosInfo3D StartPosition { get; private set; }

            /// <summary>
            /// Gets the last mouse position during <see cref="OnMouseDown"/> or <see cref="OnMouseMove"/>
            /// </summary>
            protected MousePosInfo3D LastPosition { get; private set; }

            /// <summary>
            /// Invoked when the user presses the mouse to start dragging.
            /// </summary>
            /// <param name="position">2D and 3D position information</param>
            protected abstract void MouseDownCore(MousePosInfo3D position);

            /// <summary>
            /// Invoked when the user moved his mouse during drag operation
            /// </summary>
            /// <param name="position">2D and 3D position information</param>
            /// <param name="vec2d">2D move vector</param>
            /// <param name="vec3d">3D move vector</param>
            protected abstract void MouseMoveCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d);

            /// <summary>
            /// Invoked when the user releases the mouse.
            /// </summary>
            /// <param name="position">2D and 3D position information</param>
            /// <param name="vec2d">2D move vector</param>
            /// <param name="vec3d">3D move vector</param>
            protected abstract void MouseUpCore(MousePosInfo3D position, Vector vec2d, Vector3D? vec3d);

            private void GetDragVectors(MousePosInfo3D position, out Vector vec2d, out Vector3D? vec3d)
            {
                vec2d = position.Pos2D - LastPosition.Pos2D;
                vec3d = null;
                if (position.Ray3D != null && position.Pos2D != null)
                {
                    var lastPos3d = uiState.SketchPlane.PointFromRay(LastPosition.Ray3D.Value);
                    var currPos3d = uiState.SketchPlane.PointFromRay(position.Ray3D.Value);
                    vec3d = currPos3d - lastPos3d;
                }
            }
        }

        #endregion

    }
}
