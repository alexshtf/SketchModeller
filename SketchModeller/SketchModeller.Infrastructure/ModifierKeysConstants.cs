using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SketchModeller.Infrastructure
{
    /// <summary>
    /// Defines constants for the meaning of different modifier keys in the application
    /// </summary>
    public static class ModifierKeysConstants
    {
        public const ModifierKeys ADD_SELECT_MODIFIER = ModifierKeys.Control;
        public const ModifierKeys REMOVE_SELECT_MODIFIER = ModifierKeys.Control | ModifierKeys.Shift;
    }
}
