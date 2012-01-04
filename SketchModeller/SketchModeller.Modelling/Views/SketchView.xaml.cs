using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
using SketchModeller.Infrastructure.Data;
using System.Diagnostics;
using SketchModeller.Modelling.Events;
using System.Linq;
using SketchModeller.Modelling.Editing;
using SketchModeller.Infrastructure.Services;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchView.xaml
    /// </summary>
    public partial class SketchView : UserControl
    {
        private static readonly Cursor ADD_CURSOR;
        private static readonly Cursor REMOVE_CURSOR;
        private static bool AddedOnce;
        private static bool DragStarted;
        private static Point prevpos2d;
        static SketchView()
        {
            AddedOnce = false;
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
        private readonly DuplicateEditorFactory duplicateEditorFactory;
        private readonly IDragStrategy newPrimitiveDragStrategy;
        private readonly IDragStrategy snappedDragStrategy;
        private readonly IDragStrategy curveDragStrategy;
        private readonly AssignDragStrategy assignDragStrategy;
        private IDragStrategy currentDragStrategy;

        private readonly SketchModellingView sketchModellingView;
        private readonly SketchImageView sketchImageView;
        private readonly ISnapper snapper;
        
        public SketchView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchView(SketchViewModel viewModel, UiState uiState, IEventAggregator eventAggregator, IUnityContainer container, ISnapper snapper, ILoggerFacade logger = null)
            : this()
        {
            this.logger = logger ?? new EmptyLogger();

            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            this.viewModel = viewModel;
            this.duplicateEditorFactory = new DuplicateEditorFactory(this);
            this.snapper = snapper;

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

            newPrimitiveDragStrategy = new PrimitiveDragStrategy(uiState, sketchModellingView, snapper);
            snappedDragStrategy = new SnappedDragStrategy(uiState, duplicateEditorFactory.Create(), eventAggregator);
            curveDragStrategy = new CurveDragStrategy(uiState, sketchImageView, selectionRectangle);
            assignDragStrategy = new AssignDragStrategy(uiState, primitiveCurvesRoot, sketchImageView, eventAggregator);

            eventAggregator.GetEvent<PrimitiveCurvesChangedEvent>().Subscribe(OnPrimitiveCurvesChanged);
            eventAggregator.GetEvent<GlobalShortcutEvent>().Subscribe(OnGlobalShortcut);
        }

        private void OnPrimitiveCurvesChanged(NewPrimitive primitive)
        {
            var container = primitiveCurvesRoot.ItemContainerGenerator.ContainerFromItem(primitive);
            if (container != null)
            {
                var primitiveView = 
                    container.VisualTree().OfType<NewPrimitiveCurvesControl>().FirstOrDefault();
                if (primitiveView != null)
                    primitiveView.Update();
            }
        }

        private void OnGlobalShortcut(KeyEventArgs e)
        {
            if (e.Key == Key.T)
                if (viewModel.DeletePrimitive.CanExecute(null))
                    viewModel.DeletePrimitive.Execute(null);

            if (e.Key == Key.P)
                if (viewModel.SnapPrimitive.CanExecute(null))
                    viewModel.SnapPrimitive.Execute(null);

            if (e.Key == Key.R)
                viewModel.IsPreviewing = !viewModel.IsPreviewing;

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
                projMatrix.OffsetZ = 0.2;
                camera.ProjectionMatrix = projMatrix;
            }
        }

        #region Sketch viewport mouse events

        private void vpRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDragStrategy == null)
            {
                var positionInfo = GetPosition3D(e);
                var primitiveVisual = PrimitivesPickService.PickPrimitiveVisual(viewport3d, positionInfo.Pos2D);
                if (primitiveVisual != null)
                {
                    var primitiveData = PrimitivesPickService.GetPrimitiveData(primitiveVisual);
                    viewModel.SelectPrimitive(primitiveData);

                    primitiveData.MatchClass<NewPrimitive>(_ => currentDragStrategy = newPrimitiveDragStrategy);
                    primitiveData.MatchClass<SnappedPrimitive>(_ => currentDragStrategy = snappedDragStrategy);
                    Debug.Assert(currentDragStrategy != null);

                    currentDragStrategy.OnMouseDown(GetPosition3D(e), Tuple.Create(primitiveVisual, primitiveData));
                    vpRoot.CaptureMouse();
                }
                else
                    viewModel.UnselectPrimitives();
            }
        }

        private void vpRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDragStrategy != null)
            {
                vpRoot.ReleaseMouseCapture();
                
                currentDragStrategy.OnMouseUp(GetPosition3D(e));
                currentDragStrategy = null;

                viewModel.SnapPrimitive.Execute(null);
            }
        }

        private void vpRoot_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDragStrategy == null)
            {
                if (assignDragStrategy.IsReadyToAssign)
                    currentDragStrategy = assignDragStrategy;
                else
                    currentDragStrategy = curveDragStrategy;
                currentDragStrategy.OnMouseDown(GetPosition3D(e), null);
                vpRoot.CaptureMouse();
            }
        }

        private void vpRoot_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDragStrategy == curveDragStrategy || currentDragStrategy == assignDragStrategy)
            {
                currentDragStrategy.OnMouseUp(GetPosition3D(e));
                currentDragStrategy = null;
                vpRoot.ReleaseMouseCapture();
            }
        }

        private void vpRoot_MouseMove(object sender, MouseEventArgs e)
        {
            var position3d = GetPosition3D(e);

            if (currentDragStrategy != null && currentDragStrategy.IsDragging)
                currentDragStrategy.OnMouseMove(position3d);
            else
                assignDragStrategy.EmphasizeCurves(position3d.Pos2D);
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

        #region primitives palette handling

        private PrimitiveKinds palettePrimitiveKind;
        private IInputElement paletteElement;

        private void primitivesPalette_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            paletteElement = sender as IInputElement;
            logger.Log("Primitives palette left mouse down " + paletteElement, Category.Debug, Priority.None);

            if (sender == cylinderThumb)
                palettePrimitiveKind = PrimitiveKinds.Cylinder;
            else if (sender == coneThumb)
                palettePrimitiveKind = PrimitiveKinds.Cone;
            else if (sender == sphereThumb)
                palettePrimitiveKind = PrimitiveKinds.Sphere;
            else if (sender == sgcThumb)
                palettePrimitiveKind = PrimitiveKinds.SGC;
            else if (sender == bgcThumb)
                palettePrimitiveKind = PrimitiveKinds.BGC;
            else
                logger.Log("Invalid event sender", Category.Exception, Priority.High);

            bool captureSucceeded = paletteElement.CaptureMouse();
            if (!captureSucceeded)
                logger.Log("Unable to capture mouse by a palette element " + sender, Category.Exception, Priority.None);
        }

        private void primitivesPalette_MouseMove(object sender, MouseEventArgs e)
        {
            if (paletteElement == null) // this means that the user has not started dragging an element from the palette
                return;

            var isMouseOverViewportRoot = IsMouseOverViewportRoot(e);

            // we will add a primitive and transfer the "responsibility" to the primitive drag strategy
            // so that from now on it handles mouse events
            if (isMouseOverViewportRoot) 
            {
                var pos3d = GetPosition3D(e);
                if (pos3d.Ray3D == null)
                    return;

                // we add a new primitive and pick its visual so that we can later give it to the drag strategy
                viewModel.AddNewPrimitive(palettePrimitiveKind, pos3d.Ray3D.Value);
                var primitiveVisual = PrimitivesPickService.PickPrimitiveVisual(viewport3d, pos3d.Pos2D);
                if (primitiveVisual == null)
                    return;

                var primitiveData = PrimitivesPickService.GetPrimitiveData(primitiveVisual);
                viewModel.SelectPrimitive(primitiveData);

                // we make the current mouse drag strategy to be the strategy for "new primitive" objects. This strategy
                // will be used from now on until the primitive is snapped.
                currentDragStrategy = newPrimitiveDragStrategy;
                currentDragStrategy.OnMouseDown(GetPosition3D(e), Tuple.Create(primitiveVisual, primitiveData));

                // we transfer the responsibility of capturing subsequent mouse events to the vpRoot
                paletteElement.ReleaseMouseCapture();
                vpRoot.CaptureMouse();
                paletteElement = null; 
            }
        }

        private void primitivesPalette_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (paletteElement == null)
                return;

            logger.Log("Primitives palette left mouse up " + paletteElement, Category.Debug, Priority.None);
            paletteElement.ReleaseMouseCapture();
            paletteElement = null;
        }

        private bool IsMouseOverViewportRoot(MouseEventArgs e)
        {
            var position = e.GetPosition(vpRoot);
            var rect = new Rect(0, 0, vpRoot.ActualWidth, vpRoot.ActualHeight);
            var isInVpRoot = rect.Contains(position);
            return isInVpRoot;
        }

        #endregion

        #region IDragStrategy interface

        [ContractClass(typeof(DragStragegyContract))]
        private interface IDragStrategy
        {
            /// <summary>
            /// Called when user presses the mouse to start dragging.
            /// </summary>
            /// <param name="position">2D and 3D mouse position information</param>
            /// <param name="data">Additional data needed for the drag strategy</param>
            void OnMouseDown(MousePosInfo3D position, dynamic data);


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
        private abstract class DragStragegyContract : IDragStrategy
        {
            public void OnMouseDown(MousePosInfo3D position, dynamic data)
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

            public void OnMouseDown(MousePosInfo3D position, dynamic data)
            {
                isDragging = true;
                LastPosition = StartPosition = position;
                MouseDownCore(position, data);
            }

            public void OnMouseMove(MousePosInfo3D position)
            {
                //MessageBox.Show("I am moving");
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
            protected abstract void MouseDownCore(MousePosInfo3D position, object data);

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

        #region DuplicateEditorFactory class

        private class DuplicateEditorFactory
        {
            private readonly SketchView sketchView;
            private readonly IDirectionInference directionInference;

            public DuplicateEditorFactory(SketchView sketchView)
            {
                this.sketchView = sketchView;
                directionInference = new PCADirectionInference();
            }

            public IDuplicateEditor Create()
            {
                return new DuplicateEditor(sketchView.viewModel.SketchModellingViewModel, 
                                           directionInference);
            }
        }

        #endregion

    }
}
