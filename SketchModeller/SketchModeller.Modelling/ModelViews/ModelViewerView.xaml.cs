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
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Microsoft.Practices.Unity;

namespace SketchModeller.Modelling.ModelViews
{
    /// <summary>
    /// Interaction logic for ModelViewerView.xaml
    /// </summary>
    public partial class ModelViewerView : UserControl
    {
        public ModelViewerView()
        {
            InitializeComponent();
        }

        [InjectionConstructor]
        public ModelViewerView(ModelViewerViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }

        private void OnTextboxPreviewKeyDown(object sender, KeyEventArgs e)
        {

            switch(e.Key)
            {
                case Key.Up:
                    ClickButton(up);
                    break;
                case Key.Down:
                    ClickButton(down);
                    break;
                case Key.Left:
                    ClickButton(left);
                    break;
                case Key.Right:
                    ClickButton(right);
                    break;
            }
        }

        private void ClickButton(Button button)
        {
            var peer = new ButtonAutomationPeer(button);
            var invokeProv =  peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }
    }
}
