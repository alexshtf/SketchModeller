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

namespace SketchModeller.Modelling.SketchCreator
{
    /// <summary>
    /// Interaction logic for SketchCreatorView.xaml
    /// </summary>
    public partial class SketchCreatorView 
    {
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
        }

        #region Curve drawing handling

        private Polyline currentDrawing;
        
        private void sketch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isDrawing && !isSelecting)
            {
                sketch.CaptureMouse();
                isDrawing = true;
                currentDrawing = new Polyline() { Stroke = Brushes.Blue, StrokeThickness = 1, StrokeLineJoin = PenLineJoin.Round };
                currentDrawing.Points.Add(e.GetPosition(sketch));
                sketch.Children.Add(currentDrawing);
            }
        }

        private void sketch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                sketch.ReleaseMouseCapture();
                sketch.Children.Remove(currentDrawing);
                // TODO: Do something with the points
                currentDrawing = null;
            }
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
        
        #endregion

        private void sketch_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                var pos = e.GetPosition(this);
                var rect = new Rect(pos, selectStartPos);
                selectionRect.Width = rect.Width;
                selectionRect.Height = rect.Height;
                Canvas.SetLeft(selectionRect, rect.Left);
                Canvas.SetTop(selectionRect, rect.Top);
                selectionRect.Visibility = Visibility.Visible;
            }
            else if (isDrawing)
            {
                currentDrawing.Points.Add(e.GetPosition(sketch));
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
