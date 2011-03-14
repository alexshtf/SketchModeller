using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using SketchModeller.Infrastructure;
using System.Diagnostics;
using Utils;
using System.Windows.Media;
using System.Globalization;

namespace SketchModeller.Modelling.ModelViews
{
    class NavigationRectangleFillConverter : IMultiValueConverter
    {
        public static readonly NavigationRectangleFillConverter Instance = new NavigationRectangleFillConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
            {
                Trace.TraceError("values array must be of length 2");
                return Binding.DoNothing;
            }

            if (!values.OfType<bool>().Any())
            {
                Trace.TraceError("values items must be of type boolean");
                return Binding.DoNothing;
            }

            var isKeyboardFocusWithin = (bool)values[0];
            var isFlightMode = (bool)values[1];

            if (!isFlightMode)
                return Brushes.LightGray;
            else if (isKeyboardFocusWithin)
                return Brushes.LightGreen;
            else
                return Brushes.LightBlue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ErrorMessages.ConvertBackNotSupported);
        }
    }
}
