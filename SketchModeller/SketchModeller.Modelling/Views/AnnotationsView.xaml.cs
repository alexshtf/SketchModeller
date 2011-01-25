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
    /// Interaction logic for AnnotationsView.xaml
    /// </summary>
    public partial class AnnotationsView : UserControl, IHeaderedView
    {
        private readonly AnnotationsViewModel viewModel;

        public AnnotationsView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public AnnotationsView(AnnotationsViewModel viewModel)
            : this()
        {
            this.viewModel = viewModel;
            DataContext = viewModel;
        }

        public string ViewName
        {
            get { return "Annotations"; }
        }
    }
}
