using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using SketchModeller.Infrastructure;

namespace SketchModeller
{
    class ContextMenuStyleSelector : StyleSelector
    {
        public static readonly object SimpleStyleKey = Guid.NewGuid();
        public static readonly object CheckedStyleKey = Guid.NewGuid();

        public static readonly StyleSelector Instance = new ContextMenuStyleSelector();

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is CheckedMenuCommandData)
                return (Style)Application.Current.FindResource(CheckedStyleKey);

            if (item is MenuCommandData)
                return (Style)Application.Current.FindResource(SimpleStyleKey);

            return base.SelectStyle(item, container);
        }
    }
}
