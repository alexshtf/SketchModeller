using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;

namespace SketchModeller.Modelling.Views
{
    internal interface INewPrimitiveView
    {
        NewPrimitiveViewModel ViewModel { get; }
    }

    static class NewPrimitiveViewExtensions
    {
        public static INewPrimitiveView PrimitiveViewParent(this DependencyObject source)
        {
            return source.VisualPathUp().OfType<INewPrimitiveView>().FirstOrDefault();
        }
    }
}
