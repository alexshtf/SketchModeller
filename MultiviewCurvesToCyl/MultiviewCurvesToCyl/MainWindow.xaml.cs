using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Utils;
using Microsoft.Win32;

namespace MultiviewCurvesToCyl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private MainViewModel mainViewModel;
        private bool isConstructingCurve;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel(ChooseOpenFile, ChooseSaveFile);
            DataContext = mainViewModel;

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();

            openFileDialog.DefaultExt = saveFileDialog.DefaultExt = "xml";
        }

        #region File choosing

        private string ChooseOpenFile()
        {
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value == true)
                return openFileDialog.FileName;
            else
                return null;  
        }

        private string ChooseSaveFile()
        {
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value == true)
                return saveFileDialog.FileName;
            else
                return null;
        }

        #endregion

        #region Sketching mouse events

        private void OnSketchGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !isConstructingCurve) // start sketching. capture mouse.
            {
                CastUtils.MatchClass<IInputElement>(sender, senderInputElement =>
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
                CastUtils.MatchClass<IInputElement>(sender, senderInputElement =>
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
                CastUtils.MatchClass<IInputElement>(sender, senderInputElement =>
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
                CastUtils.MatchClass<IInputElement>(sender, senderInputElement =>
                    {
                        mainViewModel.AddUnderConstructionPoint(e.GetPosition(senderInputElement));
                    });
            }
        }

        #endregion
    }
}
