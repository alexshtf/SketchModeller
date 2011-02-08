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
using SketchModeller.Infrastructure;
using Microsoft.Practices.Unity;

namespace SketchModeller.Modelling.Views
{
    /// <summary>
    /// Interaction logic for SnappedPanelView.xaml
    /// </summary>
    public partial class SnappedPanelView : UserControl, IHeaderedView
    {

        public SnappedPanelView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SnappedPanelView(SnappedPanelViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }

        public string ViewName
        {
            get { return "Snapped primitives"; }
        }
    }
}
