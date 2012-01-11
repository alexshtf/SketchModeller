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

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for EditMenuView.xaml
    /// </summary>
    public partial class EditMenuView
    {
        public EditMenuView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public EditMenuView(EditMenuViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }
    }
}
