using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using Utils;
using System.Diagnostics;

namespace MultiviewCurvesToCyl
{
    public partial class MainWindow
    {
        private class Controller : DispatcherObject
        {
            private const double FORWARD_BACK_AMOUNT = 5.0;
            private const double LEFT_RIGHT_AMOUNT = 5.0;
            private const double ROTATE_SENSITIVITY = 0.05;

            private static readonly TimeSpan TIMER_INTERVAL = TimeSpan.FromMilliseconds(25);

            private readonly MainViewModel viewModel;
            private readonly DispatcherTimer timer;
            private readonly Dictionary<Key, Action> keysToActions;

            public Controller(MainViewModel viewModel)
            {
                this.viewModel = viewModel;

                timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
                timer.Interval = TIMER_INTERVAL;
                timer.Tick += OnTimerTick;
                timer.Start();

                keysToActions = new Dictionary<Key, Action>
                {
                    {Key.W, MoveForward },
                    {Key.S, MoveBack },
                    {Key.A, MoveLeft },
                    {Key.D, MoveRight },
                };
            }

            public void RotateView(Vector diff)
            {
                diff = diff * ROTATE_SENSITIVITY;
                
                var horizontalDegrees = -diff.X;
                var verticalDegrees = -diff.Y;

                var vertTransform = new RotateTransform3D(new AxisAngleRotation3D(RightVector, verticalDegrees));
                var newUpDirection = vertTransform.Transform(viewModel.UpDirection);
                var newViewDirection = vertTransform.Transform(viewModel.ViewDirection);
                var horizTransform = new RotateTransform3D(new AxisAngleRotation3D(viewModel.UpDirection, horizontalDegrees));
                newUpDirection = horizTransform.Transform(newUpDirection);
                newViewDirection = horizTransform.Transform(newViewDirection);

                viewModel.RotateCamera(newViewDirection, newUpDirection);
            }

            private void OnTimerTick(object sender, EventArgs e)
            {
                if (viewModel.IsInNavigationMode)
                {
                    foreach (var item in keysToActions)
                    {
                        var key = item.Key;
                        var action = item.Value;

                        var state = Keyboard.GetKeyStates(item.Key);
                        if ((state & KeyStates.Down) != KeyStates.None)
                        {
                            Trace.WriteLine("Executing action for " + key);
                            action();
                        }
                    }
                }
            }

            #region Action methods

            private void MoveForward()
            {
                viewModel.ViewPosition =
                    viewModel.ViewPosition + 
                    FORWARD_BACK_AMOUNT * viewModel.ViewDirection.Normalized();
            }

            private void MoveBack()
            {
                viewModel.ViewPosition =
                    viewModel.ViewPosition -
                    FORWARD_BACK_AMOUNT * viewModel.ViewDirection.Normalized();
            }

            private void MoveLeft()
            {
                var leftVector = Vector3D.CrossProduct(viewModel.ViewDirection, viewModel.UpDirection).Normalized();
                viewModel.ViewPosition =
                    viewModel.ViewPosition -
                    LEFT_RIGHT_AMOUNT * RightVector;
            }

            private void MoveRight()
            {
                viewModel.ViewPosition =
                    viewModel.ViewPosition +
                    LEFT_RIGHT_AMOUNT * RightVector;
            }

            #endregion

            private Vector3D RightVector
            {
                get
                {
                    return Vector3D.CrossProduct(viewModel.ViewDirection, viewModel.UpDirection).Normalized();
                }
            }
        }
    }
}
