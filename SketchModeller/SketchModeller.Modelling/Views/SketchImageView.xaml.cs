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

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchImageView.xaml
    /// </summary>
    public partial class SketchImageView 
    {
        private static readonly Brush SKETCH_STROKE_NORMAL = Brushes.Black;
        private static readonly Brush SKETCH_STROKE_OVER = Brushes.Orange;

        private static readonly Transform DEFAULT_GEOMETRY_TRANSFORM;

        private SketchImageViewModel viewModel;
        private DispatcherTimer timer;

        static SketchImageView()
        {
            var tg = new TransformGroup();
            tg.Children.Add(new TranslateTransform(1, 1));
            tg.Children.Add(new ScaleTransform(256, 256));
            tg.Freeze();

            DEFAULT_GEOMETRY_TRANSFORM = tg;
        }

        public SketchImageView()
        {
            InitializeComponent();
            timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (sender, args) =>
                {

                    timer.Stop();
                };
            timer.Start();
        }

        [InjectionConstructor]
        public SketchImageView(SketchImageViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            //grid.DataContext = viewModel;
            ViewModel3DHelper.InheritViewModel(this, viewModel);
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // when visibility flags get updated
            e.Match(() => viewModel.IsSketchShown, () => 
                ShowHide(viewModel.IsSketchShown, scatterVisibilityTransform, sketchVisibilityTransform));

            // when points get updated
            e.Match(() => viewModel.Points, () =>
                {
                    // IMPORTANT!!!! We assume that viewModel.ImageWidth and viewModel.ImageHeight have the correct
                    // values at this stage. That is, when we notify about points update we already have the image data.
                    var points3d = from pnt in viewModel.Points
                                   select new Point3D(pnt.X, pnt.Y, 0);
                    scatter.Points = new Point3DCollection(points3d);
                });

            e.Match(() => viewModel.Polylines, () => AddPaths(polylinesCanvas, viewModel.Polylines, isClosed: false));
            e.Match(() => viewModel.Polygons, () => AddPaths(polygonsCanvas, viewModel.Polygons, isClosed: true));
        }

        private static void AddPaths(Canvas canvas, IEnumerable<Infrastructure.Data.PointsSequence> sequences, bool isClosed)
        {
            canvas.Children.Clear();
            if (sequences != null)
            {
                var paths = from pointsSequence in sequences
                            select CreatePath(pointsSequence, isClosed);
                foreach (var path in paths)
                    canvas.Children.Add(path);
            }
        }

        private static Path CreatePath(Infrastructure.Data.PointsSequence polylineData, bool isClosed)
        {
            var points = (from pnt in polylineData.Points
                          select new Point { X = pnt.X, Y = pnt.Y }).ToArray();

            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                context.BeginFigure(points[0], false, isClosed);
                context.PolyLineTo(points.Skip(1).ToList(), true, false);
            }
            geometry.Transform = DEFAULT_GEOMETRY_TRANSFORM;
            geometry.Freeze();

            var path = new Path();
            path.Data = geometry;
            path.StrokeThickness = 2;

            path.Bind(
                Path.StrokeProperty,
                () => path.IsMouseDirectlyOver,
                converter: isMouseOver => isMouseOver ? SKETCH_STROKE_OVER : SKETCH_STROKE_NORMAL);
 
            return path;
        }

        private void ShowHide(bool flag, params ScaleTransform3D[] visibilityTransform)
        {
            double value = flag ? 1.0 : 0;
            foreach(var item in visibilityTransform)
                item.ScaleX = item.ScaleY = item.ScaleZ = value;
        }
    }
}
