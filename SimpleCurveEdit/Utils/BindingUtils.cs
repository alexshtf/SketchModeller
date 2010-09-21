using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Utils
{
    /// <summary>
    /// Various utilities related to WPF bindings.
    /// </summary>
    public static class BindingUtils
    {
        /// <summary>
        /// Binds a dependency property of a dependency object to the specified source, source path and converter.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="prop">The binding target property.</param>
        /// <param name="path">The source path</param>
        /// <param name="source">The binding source object.</param>
        /// <param name="converter">An optional converter for the binding.</param>
        public static void Bind(this DependencyObject target, DependencyProperty prop, string path, object source, IValueConverter converter = null)
        {
            var binding = new Binding
            {
                Path      = new PropertyPath(path),
                Source    = source,
                Converter = converter,
            };
            BindingOperations.SetBinding(target, prop, binding);
        }
    }
}
