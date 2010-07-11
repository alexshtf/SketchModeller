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
using Utils;
using System.Windows.Media.Media3D;
using Controls;

namespace SimpleCurveEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isStroking;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isStroking = true;
            rootVisual.CaptureMouse();
            stroke.Points = new PointCollection();
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isStroking)
            {
                isStroking = false;
                rootVisual.ReleaseMouseCapture();

                if (stroke.Points != null && stroke.Points.Count > 1)
                {
                    var width = viewport3d.ActualWidth;
                    var height = viewport3d.ActualHeight;
                    var points3d = new Point3DCollection(
                        from point2d in stroke.Points
                        select new Point3D(point2d.X - width / 2, -point2d.Y + height / 2, 0));

                    var curve3d = new Curve3D();
                    curve3d.Positions = points3d;

                    curves.Children.Add(curve3d);

                    stroke.Points = null;
                }
            }
        }

        private void rootVisual_MouseMove(object sender, MouseEventArgs e)
        {
            if (isStroking)
            {
                var position = e.GetPosition(rootVisual);
                stroke.Points.Add(position);
            }
        }
    }
}
