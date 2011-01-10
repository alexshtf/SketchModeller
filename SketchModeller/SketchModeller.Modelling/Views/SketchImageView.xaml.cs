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

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchImageView.xaml
    /// </summary>
    public partial class SketchImageView 
    {
        private static readonly Brush SKETCH_STROKE_NORMAL;
        private static readonly Brush SKETCH_STROKE_OVER;
        private static readonly Brush SKETCH_STROKE_SELECTED;

        static SketchImageView()
        {
            SKETCH_STROKE_NORMAL = Brushes.Black;
            SKETCH_STROKE_OVER = Brushes.Orange;
            SKETCH_STROKE_SELECTED = Brushes.Navy;
        }

        private SketchImageViewModel viewModel;

        public SketchImageView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchImageView(SketchImageViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            DataContext = viewModel;
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
            path.Stroke = SKETCH_STROKE_NORMAL;
            path.DataContext = polylineData;

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

        private Point startPoint;
        private Point endPoint;
        private ISet<Path> lastUnderRectPaths = EmptySet<Path>.Instance;

        private void OnPolyRootMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // clear the actual selection
                foreach (var path in lastUnderRectPaths)
                    path.Stroke = SKETCH_STROKE_NORMAL;

                // when we start a selection process the last under-rect set is empty.
                lastUnderRectPaths = EmptySet<Path>.Instance;

                startPoint = e.GetPosition(polyRoot);
                Canvas.SetLeft(selectionRectangle, startPoint.X);
                Canvas.SetTop(selectionRectangle, startPoint.Y);
                selectionRectangle.Width = 0;
                selectionRectangle.Height = 0;
                selectionRectangle.Visibility = Visibility.Visible;

                polyRoot.CaptureMouse();
            }
        }

        private void OnPolyRootMouseMove(object sender, MouseEventArgs e)
        {
            if (selectionRectangle.Visibility == Visibility.Visible)
            {
                endPoint = e.GetPosition(polyRoot);

                var rect = new Rect(startPoint, endPoint);

                Canvas.SetLeft(selectionRectangle, rect.Left);
                Canvas.SetTop(selectionRectangle, rect.Top);
                selectionRectangle.Width = rect.Width;
                selectionRectangle.Height = rect.Height;

                var currUnderRect = FindPaths();

                var addedPaths = currUnderRect.Except(lastUnderRectPaths);
                foreach (var path in addedPaths)
                    path.Stroke = SKETCH_STROKE_OVER;

                var removedPaths = lastUnderRectPaths.Except(currUnderRect);
                foreach (var path in removedPaths)
                    path.Stroke = SKETCH_STROKE_NORMAL;

                lastUnderRectPaths = currUnderRect;
            }
        }

        private void OnPolyRootMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // hide selection visuals
                polyRoot.ReleaseMouseCapture();
                selectionRectangle.Visibility = Visibility.Collapsed;

                // perform the actual selection
                foreach (var path in lastUnderRectPaths)
                {
                    var pointsSequence = (SketchModeller.Infrastructure.Data.PointsSequence)path.DataContext;
                    pointsSequence.IsSelected = true;
                    path.Stroke = SKETCH_STROKE_SELECTED;
                }
            }
        }

        private ISet<Path> FindPaths()
        {
            var rect = new Rect(startPoint, endPoint);
            var htParams = new GeometryHitTestParameters(new RectangleGeometry(rect));

            var hitTestResults = new HashSet<Path>();
            VisualTreeHelper.HitTest(
                polyRoot,
                filterCallback: null,
                resultCallback: htResult =>
                {
                    var path = htResult.VisualHit as Path;
                    if (path != null)
                    {
                        var geometryHtResult = (GeometryHitTestResult)htResult;
                        if (geometryHtResult.IntersectionDetail.HasFlag(IntersectionDetail.FullyInside))
                            hitTestResults.Add(path);
                    }
                    return HitTestResultBehavior.Continue;
                },
                hitTestParameters: htParams);

            return hitTestResults;
        }

        private static IEnumerable<SketchModeller.Infrastructure.Data.PointsSequence> GetPointsSequences(IEnumerable<Path> paths)
        {
            Contract.Requires(paths != null);

            return from path in paths
                   select (SketchModeller.Infrastructure.Data.PointsSequence)path.DataContext;
        }

        #endregion
    }
}
