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
using Utils;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.SketchCreator
{
    /// <summary>
    /// Interaction logic for SketchCreatorView.xaml
    /// </summary>
    public partial class SketchCreatorView 
    {
        public static readonly IValueConverter PointsConverter =
            new DelegateConverter<IEnumerable<Point>>(pts => new PointCollection(pts));
        public static readonly IValueConverter StrokeConverter =
            new DelegateConverter<CurveCategories>(cat =>
            {
                switch (cat)
                {
                    case CurveCategories.Feature:
                        return Brushes.Black;
                    case CurveCategories.Silhouette:
                        return Brushes.Gray;
                    default:
                        return Binding.DoNothing;
                }
            });

        private readonly SketchCreatorViewModel viewModel;
        private bool isSelecting;
        private bool isDrawing;


        public SketchCreatorView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchCreatorView(SketchCreatorViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            DataContext = viewModel;
        }

        #region Curve drawing handling

        private void sketch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDrawing && !isSelecting)
            {
                bool canDraw = viewModel.IsFeatureMode || viewModel.IsSilhouetteMode;
                if (canDraw)
                {
                    sketch.CaptureMouse();
                    isDrawing = true;
                    currentStroke.Points.Clear();
                    currentStroke.Points.Add(e.GetPosition(sketch));
                    currentStroke.Visibility = Visibility.Visible;
                }
            }
        }

        private void sketch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                sketch.ReleaseMouseCapture();
                currentStroke.Visibility = Visibility.Collapsed;
                
                var curveCategory = CurveCategories.None;
                if (viewModel.IsFeatureMode)
                    curveCategory = CurveCategories.Feature;
                else if (viewModel.IsSilhouetteMode)
                    curveCategory = CurveCategories.Silhouette;
                
                viewModel.Curves.Add(new SketchModeller.Infrastructure.Data.Polyline
                    {
                        CurveCategory = curveCategory,
                        Points = currentStroke.Points.ToArray(),
                    });
            }
        }

        private void DrawingMouseMove(Point position)
        {
            currentStroke.Points.Add(position);
        }

        #endregion

        #region Curves selection handling

        private Point selectStartPos;

        private void sketch_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isSelecting && !isDrawing)
            {
                isSelecting = true;
                sketch.CaptureMouse();
                selectStartPos = e.GetPosition(sketch);
            }
        }

        private void sketch_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                sketch.ReleaseMouseCapture();
                selectionRect.Visibility = Visibility.Hidden;
                // TODO: Perform the actual selection
            }
        }

        private void SelectionMouseMove(Point position)
        {
            var rect = new Rect(position, selectStartPos);
            selectionRect.Width = rect.Width;
            selectionRect.Height = rect.Height;
            Canvas.SetLeft(selectionRect, rect.Left);
            Canvas.SetTop(selectionRect, rect.Top);
            selectionRect.Visibility = Visibility.Visible;
        }
        
        #endregion

        private void sketch_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
                SelectionMouseMove(e.GetPosition(sketch));
            else if (isDrawing)
                DrawingMouseMove(e.GetPosition(sketch));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
