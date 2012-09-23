using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using System.Diagnostics;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Windows.Data;

namespace SketchModeller
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        public static readonly IValueConverter WokringToCursorConverter = new WokringToCursorConverterType();

        private ShellViewModel viewModel;
        private IEventAggregator eventAggregator;

        public Shell()
        {
            InitializeComponent();
        }

        public Shell(IUnityContainer container)
            : this()
        {
            viewModel = container.Resolve<ShellViewModel>();
            eventAggregator = container.Resolve<IEventAggregator>();
            DataContext = viewModel;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            bool isComputationBreakShortcut = e.Key == Key.C && e.KeyboardDevice.Modifiers == ModifierKeys.Control;

            if (isComputationBreakShortcut || !viewModel.IsWorking)
                eventAggregator.GetEvent<GlobalShortcutEvent>().Publish(e);
        }

        private void OnDebugClick(object sender, RoutedEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private void OnContextMenuCommands(object sender, MenuCommandsEventArgs e)
        {
            if (e.MenuCommands.Count > 0)
            {
                sketchContextMenu.ItemsSource = e.MenuCommands;
                sketchContextMenu.IsOpen = true;
            }
        }

        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            sketchContextMenu.ItemsSource = null;
        }

        private void DockPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        #region WokringToCursorConverterType class

        class WokringToCursorConverterType : IValueConverter
        {

            public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is bool)
                {
                    var isWorking = (bool) value;
                    if (isWorking)
                        return Cursors.Wait;
                    else
                        return null;
                }
                else
                    return Binding.DoNothing;
            }

            public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}
