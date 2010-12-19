using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace SketchModeller.Infrastructure
{
    public class MenuCommandsEventArgs : RoutedEventArgs
    {
        public MenuCommandsEventArgs()
        {
            MenuCommands = new List<MenuCommandData>();
        }

        public MenuCommandsEventArgs(RoutedEvent routedEvent)
            : base(routedEvent)
        {
            MenuCommands = new List<MenuCommandData>();
        }

        public List<MenuCommandData> MenuCommands { get; private set; }
    }
}
