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
using System.ComponentModel;
using Utils;
using System.Windows.Threading;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchImageView.xaml
    /// </summary>
    public partial class SketchImageView 
    {
        private static readonly Brush SKETCH_STROKE_NORMAL = Brushes.Black;
        private static readonly Brush SKETCH_STROKE_CANDIDATE = Brushes.Orange;
        private static readonly Brush SKETCH_STROKE_SELECTED = Brushes.Navy;
        private static readonly Brush SKETCH_STROKE_SELECTED_CANDIDATE = Brushes.DodgerBlue;

        private static readonly Cursor ADD_CURSOR;
        private static readonly Cursor REMOVE_CURSOR;

        static SketchImageView()
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

        private readonly SketchImageViewModel viewModel;
        private readonly PathsSelectionManager pathsManager;
        private readonly DispatcherTimer modifierKeysTimer;

        public SketchImageView()
        {
            InitializeComponent();
            modifierKeysTimer = new DispatcherTimer();
            modifierKeysTimer.Interval = TimeSpan.FromMilliseconds(50);
            modifierKeysTimer.Tick += new EventHandler(OnModifierKeysTimerTick);
        }

        [InjectionConstructor]
        public SketchImageView(SketchImageViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            DataContext = viewModel;

            pathsManager = new PathsSelectionManager(polyRoot, selectionRectangle);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            e.Match(() => viewModel.Polylines, () => ReplacePaths(polyRoot, viewModel.Polylines, isClosed: false, vgKind: VGKind.Polyline));
            e.Match(() => viewModel.Polygons, () => ReplacePaths(polyRoot, viewModel.Polygons, isClosed: true, vgKind: VGKind.Polygon));
        }

        private void ReplacePaths(Canvas canvas, IEnumerable<Infrastructure.Data.PointsSequence> sequences, bool isClosed, VGKind vgKind)
        {
            // remove all old paths having the same VGKind
            var indicesToRemove =
                (from item in canvas.Children.Cast<object>().ZipIndex()
                 where item.Value is Path
                 let path = (Path)item.Value
                 where GetVGKind(path) == vgKind
                 select item.Index
                ).ToArray();
            Array.Reverse(indicesToRemove);

            foreach (var index in indicesToRemove)
                canvas.Children.RemoveAt(index);

            // add new paths
            if (sequences != null)
            {
                var paths = from pointsSequence in sequences
                            select CreatePath(pointsSequence, isClosed, vgKind);
                foreach (var path in paths)
                    canvas.Children.Add(path);
            }
        }

        private Path CreatePath(Infrastructure.Data.PointsSequence polylineData, bool isClosed, VGKind vgKind)
        {
            var points = (from pnt in polylineData.Points
                          select new Point { X = pnt.X, Y = pnt.Y }).ToArray();

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(points[0], false, isClosed);
                context.PolyLineTo(points.Skip(1).ToList(), true, false);
            }

            var scaleTransform = new ScaleTransform();
            scaleTransform.Bind(ScaleTransform.ScaleXProperty, () => polyRoot.ActualWidth, width => width / 2);
            scaleTransform.Bind(ScaleTransform.ScaleYProperty, () => polyRoot.ActualHeight, height => height / 2);

            var translateTransform = new TranslateTransform();
            translateTransform.Bind(TranslateTransform.XProperty, () => polyRoot.ActualWidth, width => width / 2);
            translateTransform.Bind(TranslateTransform.YProperty, () => polyRoot.ActualHeight, height => height / 2);

            var transformGroup = new TransformGroup { Children = { scaleTransform, translateTransform } };
            geometry.Transform = transformGroup;

            var path = new Path();
            path.Data = geometry;
            path.StrokeThickness = 2;
            path.DataContext = polylineData;
            BindingOperations.SetBinding(path, Path.StrokeProperty, new Binding
                {
                    Path = new PropertyPath(PathsSelectionManager.SelectionStateProperty),
                    Source = path,
                    Converter = new DelegateConverter<SelectionState>(selectionState =>
                                {
                                    switch (selectionState)
                                    {
                                        case SelectionState.Unselected:
                                            return SKETCH_STROKE_NORMAL;
                                        case SelectionState.Candidate:
                                            return SKETCH_STROKE_CANDIDATE;
                                        case SelectionState.Selected:
                                            return SKETCH_STROKE_SELECTED;
                                        case SelectionState.Selected | SelectionState.Candidate:
                                            return SKETCH_STROKE_SELECTED_CANDIDATE;
                                        default:
                                            Debug.Fail("Invalid selection state");
                                            return null;
                                    }
                                }),
                });

            SetVGKind(path, vgKind);
            return path;
        }

        #region VGKind attached property

        private static readonly DependencyProperty VGKindProperty = DependencyProperty.RegisterAttached("VGKind", typeof(VGKind), typeof(SketchImageView));

        private static VGKind GetVGKind(Path path)
        {
            return (VGKind)path.GetValue(VGKindProperty);
        }

        private static void SetVGKind(Path path, VGKind value)
        {
            path.SetValue(VGKindProperty, value);
        }

        #endregion

        #region VGKind enum
        
        private enum VGKind
        {
            Unknown,
            Polyline,
            Polygon,
        }

        #endregion

        #region Selection mouse events

        private void OnPolyRootMouseDown(object sender, MouseButtonEventArgs e)
        {
            pathsManager.MouseDown(e);
        }

        private void OnPolyRootMouseMove(object sender, MouseEventArgs e)
        {
            pathsManager.MouseMove(e);
        }

        private void OnPolyRootMouseUp(object sender, MouseButtonEventArgs e)
        {
            pathsManager.MouseUp(e);
        }

        #endregion

        #region modifier keys cursor

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            modifierKeysTimer.Start();
            UpdateCursorFromModifierKeys();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            modifierKeysTimer.Stop();
        }

        private void OnModifierKeysTimerTick(object sender, EventArgs e)
        {
            UpdateCursorFromModifierKeys();
        }

        private void UpdateCursorFromModifierKeys()
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                this.Cursor = ADD_CURSOR;
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                this.Cursor = REMOVE_CURSOR;
            else
                this.Cursor = null;
        }

        #endregion
    }
}
