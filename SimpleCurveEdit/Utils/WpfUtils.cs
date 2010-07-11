using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;

namespace Utils
{
    public static class WpfUtils
    {
        public static Viewport3D GetViewport(this Visual3D visual3d)
        {
            var path = VisualPathUp(visual3d);
            return path.OfType<Viewport3D>().FirstOrDefault();
        }

        public static IEnumerable<object> VisualPathUp(this DependencyObject reference)
        {
            while (reference != null)
            {
                yield return reference;
                reference = VisualTreeHelper.GetParent(reference);
            }
        }
    }
}
