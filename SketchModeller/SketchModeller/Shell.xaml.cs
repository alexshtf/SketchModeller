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
using SketchModeller.Infrastructure;

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
                var sketchPlane = viewModel.SketchPlane;

                var fovRadians = camera.FieldOfView * Math.PI / 180;
                var height = sketchPlane.Height * 1.5; // add 50% height.
                var distance = height / Math.Tan(fovRadians);

                var normal = sketchPlane.Normal.Normalized();
                var center = sketchPlane.Center;
                var position = sketchPlane.Center - distance * normal;

                camera.Position = position;
                camera.LookDirection = normal;
                camera.UpDirection = sketchPlane.YAxis.Normalized();
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

        private void OnContextMenuCommands(object sender, MenuCommandsEventArgs e)
        {
            if (e.MenuCommands.Count > 0)
            {
                sketchContextMenu.ItemsSource = e.MenuCommands;
                sketchContextMenu.IsOpen = true;
            }
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            sketchContextMenu.ItemsSource = null;
        }
    }
}
