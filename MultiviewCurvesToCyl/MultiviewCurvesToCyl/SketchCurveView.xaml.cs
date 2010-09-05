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

namespace MultiviewCurvesToCyl
{
    /// <summary>
    /// Interaction logic for SketchCurveView.xaml
    /// </summary>
    public partial class SketchCurveView : UserControl
    {
        public SketchCurveView()
        {
            InitializeComponent();
        }

        private SketchCurveViewModel ViewModel
        {
            get { return DataContext as SketchCurveViewModel; } // we assume that the data-context is our view model
        }
    }
}
