using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Infrastructure
{
    public static class RoutedEvents
    {
        public static RoutedEvent ContextMenuCommandsEvent =
            EventManager.RegisterRoutedEvent(
                "ContextMenuCommands", 
                RoutingStrategy.Bubble, 
                typeof(EventHandler<MenuCommandsEventArgs>), 
                typeof(RoutedEvents));

        public static void AddContextMenuCommandsHandler(DependencyObject target, EventHandler<MenuCommandsEventArgs> handler)
        {
            var inputElement = target as IInputElement;
            if (inputElement != null)
                inputElement.AddHandler(ContextMenuCommandsEvent, handler);
        }

        public static void RemoveContextMenuCommandsHandler(DependencyObject target, EventHandler<MenuCommandsEventArgs> handler)
        {
            var inputElement = target as IInputElement;
            if (inputElement != null)
                inputElement.RemoveHandler(ContextMenuCommandsEvent, handler);
        }
    }
}
