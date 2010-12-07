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
    /// Interaction logic for SketchPlanesView.xaml
    /// </summary>
    public partial class SketchPlanesView : UserControl, IHeaderedView
    {
        private object lastSelected;
        private Blocker lastSelectedBlocker;

        public SketchPlanesView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public SketchPlanesView(SketchPlanesViewModel viewModel)
            :  this()
        {
            DataContext = viewModel;
        }

        public string ViewName
        {
            get { return "Sketch planes"; }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = (ListBox)sender;
            if (listbox.SelectedItem == null)
                listbox.SelectedItem = lastSelected;
            lastSelected = listbox.SelectedItem;
        }
    }
}
