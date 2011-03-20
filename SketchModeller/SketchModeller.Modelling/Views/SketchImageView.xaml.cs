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
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchImageView.xaml
    /// </summary>
    public partial class SketchImageView
    {
        private static readonly Brush SKETCH_STROKE_UNCATEGORIZED = Brushes.Red;
        private static readonly Brush SKETCH_STROKE_FEATURE = Brushes.Black;
        private static readonly Brush SKETCH_STROKE_SILHOUETTE = Brushes.DarkGray;

        private readonly SketchImageViewModel viewModel;

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
            path.DataContext = polylineData;
            BindingOperations.SetBinding(path, Path.StrokeProperty, new Binding
                {
                    Path = new PropertyPath("CurveCategory"),
                    Converter = new DelegateConverter<CurveCategories>(category =>
                    {
                        switch (category)
                        {
                            case CurveCategories.None:
                                return SKETCH_STROKE_UNCATEGORIZED;
                            case CurveCategories.Feature:
                                return SKETCH_STROKE_FEATURE;
                            case CurveCategories.Silhouette:
                                return SKETCH_STROKE_SILHOUETTE;
                            default:
                                return Binding.DoNothing;
                        }
                    }),
                });
            BindingOperations.SetBinding(path, Path.StrokeThicknessProperty, new Binding
                {
                    Path = new PropertyPath("IsSelected"),
                    Converter = new DelegateConverter<bool>(isSelected =>
                    { 
                        if (isSelected)
                            return 3;
                        else
                            return 1;
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

        public void SelectCurves(Rect rect)
        {
            var htParams = new GeometryHitTestParameters(new RectangleGeometry(rect));
            var results = new HashSet<Path>();
            VisualTreeHelper.HitTest(
                this,
                null, // filter callback
                htResult => // result callback
                {
                    var path = htResult.VisualHit as Path;
                    if (path != null)
                        results.Add(path);
                    return HitTestResultBehavior.Continue;
                },
                htParams);

            var selectedSequences = results.Select(x => x.DataContext).OfType<PointsSequence>();

            var currModifiers = Keyboard.Modifiers;
            if (currModifiers == ModifierKeysConstants.ADD_SELECT_MODIFIER)
                AddToSelection(selectedSequences);
            else if (currModifiers == ModifierKeysConstants.REMOVE_SELECT_MODIFIER)
                RemoveFromSelection(selectedSequences);
            else
                ReplaceSelection(selectedSequences);
        }

        private void AddToSelection(IEnumerable<PointsSequence> selectedSequences)
        {
            foreach (var seq in selectedSequences)
                seq.IsSelected = true;
        }

        private void RemoveFromSelection(IEnumerable<PointsSequence> selectedSequences)
        {
            foreach (var seq in selectedSequences)
                seq.IsSelected = false;
        }

        private void ReplaceSelection(IEnumerable<PointsSequence> selectedSequences)
        {
            var allSequences = viewModel.Polygons.Cast<PointsSequence>().Concat(viewModel.Polylines);
            var oldSelectedSequences = allSequences.Where(seq => seq.IsSelected).ToArray();
            
            var toUnSelect = oldSelectedSequences.Except(selectedSequences);
            var toSelect = selectedSequences.Except(oldSelectedSequences);

            foreach (var seq in toUnSelect)
                seq.IsSelected = false;
            foreach (var seq in toSelect)
                seq.IsSelected = true;
        }
    }
}
