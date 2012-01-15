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
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System.ComponentModel;
using Utils;
using System.Diagnostics;
using Petzold.Media3D;
using SketchModeller.Infrastructure;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;

namespace SketchModeller
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
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
            base.OnKeyDown(e);
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
    }
}
