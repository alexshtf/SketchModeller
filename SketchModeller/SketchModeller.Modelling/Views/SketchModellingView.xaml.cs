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
using Microsoft.Practices.Prism.ViewModel;
using SketchModeller.Infrastructure.Data;
using Utils;
using SketchModeller.Modelling;
using Controls;
using System.Windows.Media.Media3D;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Prism.Logging;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchModellingView.xaml
    /// </summary>
    public partial class SketchModellingView
    {
        private SketchModellingViewModel viewModel;

        public SketchModellingView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchModellingView(SketchModellingViewModel viewModel, ILoggerFacade logger)
            : this()
        {
            this.viewModel = viewModel;

            cloningVisual3D.ItemsSource = viewModel.NewPrimitiveViewModels;
            cloningVisual3D.Visual3DFactory = new Visual3DFactory(logger);

            snappedCloningVisual3d.ItemsSource = viewModel.SnappedPrimitives;
            snappedCloningVisual3d.Visual3DFactory = SketchModeller.Modelling.ModelViews.ModelViewerSnappedFactory.Instance;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var source = e.Source as DependencyObject;
                var newPrimitveView = source.PrimitiveViewParent();
                if (newPrimitveView != null)
                {
                    var newPrimitiveViewModel = newPrimitveView.ViewModel;
                    viewModel.Delete(newPrimitiveViewModel);
                }
            }
        }

        class Visual3DFactory : IVisual3DFactory
        {
            private ILoggerFacade logger;

            public Visual3DFactory(ILoggerFacade logger)
            {
                this.logger = logger;
            }

            public Visual3D Create(object item)
            {
                Visual3D result = null;
                
                item.MatchClass<NewCylinderViewModel>(
                    viewModel => result = new NewCylinderView(viewModel, logger));

                Contract.Assume(result != null);
                return result;
            }
        }
    }
}
