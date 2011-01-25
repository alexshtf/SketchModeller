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
using Petzold.Media3D;
using System.ComponentModel;

using Utils;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchView.xaml
    /// </summary>
    public partial class SketchView : UserControl
    {
        private readonly SketchViewModel viewModel;

        public SketchView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchView(SketchViewModel viewModel, IUnityContainer container)
            : this()
        {
            DataContext = viewModel;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            this.viewModel = viewModel;

            var sketchModellingView = 
                container.Resolve<SketchModellingView>(
                    new DependencyOverride<SketchModellingViewModel>(viewModel.SketchModellingViewModel));
            root3d.Children.Add(sketchModellingView);

            var sketchImageView =
                container.Resolve<SketchImageView>(
                    new DependencyOverride<SketchImageViewModel>(viewModel.SketchImageViewModel));
            root.Children.Insert(0, sketchImageView);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                var mousePos = e.GetPosition(viewport3d);
                LineRange lineRange;
                if (ViewportInfo.Point2DtoPoint3D(viewport3d, mousePos, out lineRange))
                    viewModel.OnSketchClick(lineRange.Point1, lineRange.Point2);

            }  
        }

        void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Match(() => viewModel.SketchPlane))
            {
                var sketchPlane = viewModel.SketchPlane;

                var normal = sketchPlane.Normal.Normalized();
                var center = sketchPlane.Center;
                var position = sketchPlane.Center - 50 * normal;

                var lookAt = MathUtils3D.LookAt(position, normal, sketchPlane.YAxis.Normalized());
                camera.ViewMatrix = lookAt;

                var projMatrix = Matrix3D.Identity;
                projMatrix.M33 = 0.0001;
                //projMatrix.OffsetZ = -lookAt.OffsetZ + 0.5;
                projMatrix.OffsetZ = 0.2;
                camera.ProjectionMatrix = projMatrix;
            }
        }
    }
}
