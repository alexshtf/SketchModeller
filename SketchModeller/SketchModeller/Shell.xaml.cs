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
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System.ComponentModel;
using Utils;
using System.Diagnostics;
using Petzold.Media3D;

namespace SketchModeller
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        private ShellViewModel viewModel;

        public Shell()
        {
            InitializeComponent();
        }

        public Shell(IUnityContainer container)
            : this()
        {
            viewModel = container.Resolve<ShellViewModel>();
            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Match(() => viewModel.SketchPlane))
            {
               // Trace.Fail("TODO: Update camera from SketchPlane");
            }
        }

        private void OnDebugClick(object sender, RoutedEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private void OnSketchMouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(viewport3d);
            LineRange lineRange;
            if (ViewportInfo.Point2DtoPoint3D(viewport3d, mousePos, out lineRange))
                viewModel.OnSketchClick(lineRange.Point1, lineRange.Point2);
            
        }

        private void OnSketchMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnSketchMouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
