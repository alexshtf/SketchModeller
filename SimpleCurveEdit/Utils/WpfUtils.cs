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

        public static IEnumerable<Point3D> Transform(this IEnumerable<Point3D> source, Transform3D transform)
        {
            return from pnt in source
                   select transform.Transform(pnt);
        }

        public static IEnumerable<object> VisualTree(this DependencyObject parent)
        {
            yield return parent;
            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foreach(var item in child.VisualTree())
                    yield return item;
            }
        }
    }
}
