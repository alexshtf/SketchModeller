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
        public SketchModellingView(SketchModellingViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            cloningVisual3D.ItemsSource = viewModel.NewPrimitiveViewModels;
            cloningVisual3D.Visual3DFactory = new Visual3DFactory();
        }

        class Visual3DFactory : IVisual3DFactory
        {
            public Visual3D Create(object item)
            {
                Visual3D result = null;
                
                item.MatchClass<NewCylinderViewModel>(
                    viewModel => result = new NewCylinderView(viewModel));

                Contract.Assume(result != null);
                return result;
            }
        }
    }
}
