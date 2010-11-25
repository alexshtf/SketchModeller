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
using Microsoft.Win32;
using Microsoft.Practices.Unity;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for OpenImageView.xaml
    /// </summary>
    public partial class OpenImageView : UserControl
    {
        private OpenImageViewModel viewModel;

        public OpenImageView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public OpenImageView(OpenImageViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            DataContext = viewModel;
        }
    }
}
