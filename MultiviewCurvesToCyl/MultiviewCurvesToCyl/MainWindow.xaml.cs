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
using Utils;

namespace MultiviewCurvesToCyl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;
        private bool isConstructingCurve;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
        }

        #region Sketching mouse events

        private void OnSketchGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !isConstructingCurve) // start sketching. capture mouse.
            {
                CastUtils.DoWithClass<IInputElement>(sender, senderInputElement =>
                    {
                        isConstructingCurve = true;

                        bool success = senderInputElement.CaptureMouse();
                        if (!success)
                            Trace.TraceError("Failed capturing mouse. Expect bad UI behavior.");

                        mainViewModel.AddUnderConstructionPoint(e.GetPosition(senderInputElement));

                        Trace.TraceInformation("Started curve construction with left mouse click");
                    });
            }
            else if (e.ChangedButton == MouseButton.Right && isConstructingCurve)
            {
                CastUtils.DoWithClass<IInputElement>(sender, senderInputElement =>
                    {
                        senderInputElement.ReleaseMouseCapture();
                        mainViewModel.DiscardUnderConstructionCurve();
                        isConstructingCurve = false;

                        Trace.TraceInformation("Cancelled curve construction with right mouse click.");
                    });
            }
        }

        private void OnSketchGridMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && isConstructingCurve)
            {
                CastUtils.DoWithClass<IInputElement>(sender, senderInputElement =>
                    {
                        senderInputElement.ReleaseMouseCapture();
                        mainViewModel.CommitUnderConstructionCurve();
                        isConstructingCurve = false;

                        Trace.TraceInformation("Finished curve construction with releasing left mouse.");
                    });
            }
        }

        private void OnSketchGridMouseMove(object sender, MouseEventArgs e)
        {
            if (isConstructingCurve)
            {
                CastUtils.DoWithClass<IInputElement>(sender, senderInputElement =>
                    {
                        mainViewModel.AddUnderConstructionPoint(e.GetPosition(senderInputElement));
                    });
            }
        }

        #endregion
    }
}
