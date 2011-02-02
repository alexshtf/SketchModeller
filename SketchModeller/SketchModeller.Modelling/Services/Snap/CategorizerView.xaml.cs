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
using System.Collections.Specialized;
using System.Diagnostics;
using Utils;
using System.Diagnostics.Contracts;
using PointsSequence = SketchModeller.Infrastructure.Data.PointsSequence;

namespace SketchModeller.Modelling.Services.Snap
{
    /// <summary>
    /// Interaction logic for CategorizerView.xaml
    /// </summary>
    public partial class CategorizerView
    {
        private static readonly Brush[] CATEGORY_BRUSHES;

        private CategorizerViewModel viewModel;
        private PathBrushConverter pathBrushConverter;
        private Dictionary<Snapper.CurveCategory, Brush> categoryBrushes;

        static CategorizerView()
        {
            CATEGORY_BRUSHES = new Brush[]
            {
                Brushes.Orange,
                Brushes.Purple,
                Brushes.Green,
                Brushes.SandyBrown,
                Brushes.SandyBrown,
            };
        }

        public CategorizerView()
        {
            InitializeComponent();
            pathBrushConverter = (PathBrushConverter)Resources["PathBrushConverter"];
        }

        [InjectionConstructor]
        public CategorizerView(CategorizerViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
            this.viewModel = viewModel;

            // we assume that categories are updated BEFORE paths are added.
            viewModel.CategoriesChanged += new EventHandler(viewModel_CategoriesChanged);
            viewModel.AssignmentChanged += new EventHandler(viewModel_AssignmentChanged);
            viewModel.Finished += new EventHandler(viewModel_Finished);
        }

        private void viewModel_Finished(object sender, EventArgs e)
        {
            Close();
        }

        #region view model event handlers

        private void viewModel_AssignmentChanged(object sender, EventArgs e)
        {
            Action setCategories = () =>
                {
                    foreach (var sequence in viewModel.Sequences)
                    {
                        // get the category assigned to this sequence
                        Snapper.CurveCategory category;
                        viewModel.Result.TryGetValue(sequence, out category);

                        // get the visual item assigned to this sequence.
                        var container = paths.ItemContainerGenerator.ContainerFromItem(sequence);
                        Debug.Assert(container != null); // we should have a visual object for each item. we do not use virtualization

                        // find the polygon/polyline path under the above container
                        var polygon = container.VisualTree().OfType<Polygon>().FirstOrDefault();
                        var polyline = container.VisualTree().OfType<Polyline>().FirstOrDefault();
                        Contract.Assume(polyline != null || polygon != null); // we should have a polyline or a polygon for each item

                        // set the category of the path so that the appropriate visual changed will be reflected.
                        var drawing = polygon != null ? (DependencyObject)polygon
                                                      : (DependencyObject)polyline;
                        SetCategory(drawing, category);
                    }
                };
            Dispatcher.BeginInvoke(setCategories);
        }

        private void viewModel_CategoriesChanged(object sender, EventArgs e)
        {
            categoryBrushes = new Dictionary<Snapper.CurveCategory, Brush>();
            foreach (var i in System.Linq.Enumerable.Range(0, viewModel.Categories.Count))
                categoryBrushes[viewModel.Categories[i]] = CATEGORY_BRUSHES[i];

            categoriesLegend.ItemsSource = categoryBrushes;
            pathBrushConverter.UpdateCategories(categoryBrushes);
        }

        #endregion

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #region selection handling

        private Point startMousePos;
        private bool isSelecting;

        private void paths_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isSelecting = true;
                startMousePos = e.GetPosition(selectCanvas);
                paths.CaptureMouse();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                isSelecting = false;
                selectRectangle.Visibility = Visibility.Collapsed;
                paths.ReleaseMouseCapture();
            }
        }

        private void paths_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && isSelecting)
            {
                isSelecting = false;
                selectRectangle.Visibility = Visibility.Collapsed;
                paths.ReleaseMouseCapture();

                var pos = e.GetPosition(selectCanvas);
                var bounds = GetBounds(pos);

                var allPaths = GetAllPaths();
                foreach (var path in allPaths)
                    SetIsSelected(path, false);

                var toBeSelectedPaths = GetPathsUnderRect(bounds);
                foreach (var path in toBeSelectedPaths)
                    SetIsSelected(path, true);

                var selectedSequences = toBeSelectedPaths.Select(path => GetPointsSequence(path));
                viewModel.UpdateSelection(selectedSequences.ToArray());
            }
        }

        private PointsSequence GetPointsSequence(DependencyObject path)
        {
            var container =
                path
                .VisualPathUp()
                .OfType<FrameworkElement>()
                .Where(x => x.DataContext is PointsSequence)
                .First();

            return container.DataContext as PointsSequence;
        }

        private void paths_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                var pnt = e.GetPosition(selectCanvas);
                var bounds = GetBounds(pnt);

                Canvas.SetLeft(selectRectangle, bounds.Left);
                Canvas.SetTop(selectRectangle, bounds.Top);
                selectRectangle.Width = bounds.Width;
                selectRectangle.Height = bounds.Height;

                selectRectangle.Visibility = Visibility.Visible;
            }
        }

        private Rect GetBounds(Point pnt)
        {
            return new Rect(startMousePos, pnt);
        }

        private IEnumerable<DependencyObject> GetAllPaths()
        {
            var polylines = paths.VisualTree().OfType<Polygon>().Cast<DependencyObject>();
            var polygons = paths.VisualTree().OfType<Polyline>().Cast<DependencyObject>();
            return polylines.Concat(polygons).ToArray();
        }

        private IEnumerable<DependencyObject> GetPathsUnderRect(Rect rect)
        {
            var result = new List<DependencyObject>();
            var hitTestParams = new GeometryHitTestParameters(new RectangleGeometry(rect));
            VisualTreeHelper.HitTest(
                reference: paths,
                filterCallback: null,
                resultCallback: htResult => ExtractHitTestResult(result, htResult),
                hitTestParameters: hitTestParams);

            return result;
        }

        private static HitTestResultBehavior ExtractHitTestResult(List<DependencyObject> result, HitTestResult htResult)
        {
            var hitPolygon = htResult.VisualHit as Polygon;
            var hitPolyline = htResult.VisualHit as Polyline;
            if (hitPolygon == null && hitPolyline == null)
                return HitTestResultBehavior.Continue;

            Contract.Assume(hitPolygon != null || hitPolyline != null);
            var hitDp = hitPolygon != null ? (DependencyObject)hitPolygon
                                           : (DependencyObject)hitPolyline;

            result.Add(hitDp);
            return HitTestResultBehavior.Continue;
        }

        #endregion

        #region IsSelected attached property

        private static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(CategorizerView));

        private static void SetIsSelected(DependencyObject target, bool value)
        {
            target.SetValue(IsSelectedProperty, value);
        }

        private static bool GetIsSelected(DependencyObject target)
        {
            return (bool)target.GetValue(IsSelectedProperty);
        }

        #endregion

        #region Category attached property

        private static readonly DependencyProperty CategoryProperty =
            DependencyProperty.RegisterAttached("Category", typeof(Snapper.CurveCategory), typeof(CategorizerView));

        private static void SetCategory(DependencyObject target, Snapper.CurveCategory value)
        {
            target.SetValue(CategoryProperty, value);
        }

        private static Snapper.CurveCategory GetCategory(DependencyObject target)
        {
            return (Snapper.CurveCategory)target.GetValue(CategoryProperty);
        }

        #endregion
    }
}
