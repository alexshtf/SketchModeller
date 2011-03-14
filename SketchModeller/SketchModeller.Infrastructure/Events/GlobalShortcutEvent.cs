using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Events;
using System.Windows.Input;

namespace SketchModeller.Infrastructure.Events
{
    /// <summary>
    /// Fired when a key is pressed at any level in the application. Useful for handling global keyboard
    /// shortcuts
    /// </summary>
    public class GlobalShortcutEvent : CompositePresentationEvent<KeyEventArgs>
    {
    }
}
