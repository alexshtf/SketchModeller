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
using System.Windows.Threading;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;

namespace SketchModeller.Modelling.ModelViews
{
    /// <summary>
    /// Interaction logic for ModelViewerView.xaml
    /// </summary>
    public partial class ModelViewerView : UserControl
    {
        private readonly DispatcherTimer navigationTimer;
        private readonly ModelViewerViewModel viewModel;
        private readonly ILoggerFacade logger;
        private readonly Dictionary<Key, ICommand> keyDownCommands;

        public ModelViewerView()
        {
            InitializeComponent();
            logger = new EmptyLogger();
            keyDownCommands = new Dictionary<Key, ICommand>();
        }

        [InjectionConstructor]
        public ModelViewerView(ModelViewerViewModel viewModel, ILoggerFacade logger)
            : this()
        {
            this.viewModel = viewModel;
            DataContext = viewModel;
            navigationTimer = new DispatcherTimer();
            navigationTimer.Interval = TimeSpan.FromMilliseconds(20);
            navigationTimer.Tick += new EventHandler(OnNavigationTimerTick);
            this.logger = logger;

            keyDownCommands[Key.Up] = viewModel.LookUp;
            keyDownCommands[Key.Left] = viewModel.LookLeft;
            keyDownCommands[Key.Right] = viewModel.LookRight;
            keyDownCommands[Key.Down] = viewModel.LookDown;
            keyDownCommands[Key.W] = viewModel.MoveForward;
            keyDownCommands[Key.A] = viewModel.MoveLeft;
            keyDownCommands[Key.D] = viewModel.MoveRight;
            keyDownCommands[Key.S] = viewModel.MoveBack;
        }

        private void OnNavigationTimerTick(object sender, EventArgs e)
        {
            foreach (var item in keyDownCommands)
            {
                var key = item.Key;
                var command = item.Value;
                if (Keyboard.IsKeyDown(key) && command != null && command.CanExecute(null))
                    command.Execute(null);
            }
        }

        private void OnRectangleMouseDown(object sender, MouseButtonEventArgs e)
        {
            var inputElement = sender as IInputElement;
            logger.Log("Focusing navigation panel", Category.Debug, Priority.None);
            bool success = inputElement.Focus();
            if (!success)
                logger.Log("Error focusing navigation panel", Category.Warn, Priority.None);
        }

        private void OnNavigationPanelKeyDown(object sender, KeyEventArgs e)
        {
            // we want the control to ignore arrow keys to prevent focus navigation when they are pressed.
            var arrowKeys = new Key[] { Key.Left, Key.Right, Key.Up, Key.Down };
            if (arrowKeys.Contains(e.Key))
                e.Handled = true;
        }

        private void OnNavigationPanelGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            navigationTimer.Start();
        }

        private void OnNavigationPanelLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            navigationTimer.Stop();
        }
    }
}
