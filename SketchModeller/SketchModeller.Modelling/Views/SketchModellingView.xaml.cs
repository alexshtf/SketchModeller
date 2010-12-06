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

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SketchModellingView.xaml
    /// </summary>
    public partial class SketchModellingView
    {
        public SketchModellingView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchModellingView(SketchModellingViewModel viewModel)
            : this()
        {
            ViewModel3DHelper.InheritViewModel(this, viewModel);
        }
    }
}
