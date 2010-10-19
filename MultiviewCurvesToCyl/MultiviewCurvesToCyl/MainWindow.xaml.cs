using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Utils;
using Microsoft.Win32;
using System;
using Petzold.Media3D;
using System.ComponentModel;

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
        private Controller controller;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel(ChooseOpenFile, ChooseSaveFile);
            DataContext = mainViewModel;

            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();

            openFileDialog.DefaultExt = saveFileDialog.DefaultExt = "xml";
            UpdateTotalMatrix();

            controller = new Controller(mainViewModel);

            mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        }

        private void OnMainViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            e.Match(() => mainViewModel.IsInNavigationMode, NavigationModeChangeHandler);
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
            if (mainViewModel.IsInNavigationMode)
                return;

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
            if (mainViewModel.IsInNavigationMode)
                return;

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
            if (mainViewModel.IsInNavigationMode)
                return;

            if (isConstructingCurve)
            {
                CastUtils.MatchClass<IInputElement>(sender, senderInputElement =>
                    {
                        mainViewModel.AddUnderConstructionPoint(e.GetPosition(senderInputElement));
                    });
            }
        }

        #endregion

        #region camera related

        private void OnViewportInfoChanged(object sender, EventArgs e)
        {
            UpdateTotalMatrix();
        }

        private void OnViewport3DSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTotalMatrix();
        }

        private void OnCameraChanged(object sender, EventArgs e)
        {
            UpdateTotalMatrix();
        }

        private void UpdateTotalMatrix()
        {
            if (mainViewModel != null)
            {
                var viewportInfo = new ViewportInfo { Viewport3D = viewport3d };
                mainViewModel.TotalCameraMatrix = viewportInfo.Transform;
            }
        }

        #endregion

        private void NavigationModeChangeHandler()
        {
            if (mainViewModel.IsInNavigationMode)
                viewport3dContainer.CaptureMouse();
            else
                viewport3dContainer.ReleaseMouseCapture();
        }

        private Point lastPoint;

        private void OnViewport3DMouseDown(object sender, MouseButtonEventArgs e)
        {
            lastPoint = e.GetPosition(viewport3d);
        }

        private void OnViewport3DMouseMove(object sender, MouseEventArgs e)
        {
            if (mainViewModel.IsInNavigationMode && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPos = e.GetPosition(viewport3d);
                var diff = currentPos - lastPoint;
                controller.RotateView(diff);

                lastPoint = currentPos;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
                mainViewModel.IsInNavigationMode = false;
        }

        private void rootPanel_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}
