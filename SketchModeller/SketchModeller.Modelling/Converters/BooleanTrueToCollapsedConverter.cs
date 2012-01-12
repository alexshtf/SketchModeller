using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace SketchModeller.Modelling.Converters
{
    public class BooleanTrueToCollapsedConverter : IValueConverter
    {
        public static IValueConverter Instance = new BooleanTrueToCollapsedConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                bool flag = (bool)value;
                if (flag)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else
                return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
