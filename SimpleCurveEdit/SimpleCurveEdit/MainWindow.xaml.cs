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
namespace SimpleCurveEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;


        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainViewModel();
            viewModel.Tools.Add(new StrokeTool(stroke, viewport3d, curves));
            viewModel.Tools.Add(new RotateTool(curvesTransform, curves));
            viewModel.Tools.Add(new SnapTool(snapStroke, viewport3d, curves));

            viewModel.CurrentTool = viewModel.Tools[0];

            DataContext = viewModel;
        }

        private void OnGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.CurrentTool.MouseDown(e.GetPosition(viewport3d));
            rootVisual.CaptureMouse();
        }

        private void OnGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.CurrentTool.MouseUp(e.GetPosition(viewport3d));
            rootVisual.ReleaseMouseCapture();
        }

        private void OnGridMouseMove(object sender, MouseEventArgs e)
        {
            viewModel.CurrentTool.MouseMove(e.GetPosition(viewport3d));
        }
    }
}
