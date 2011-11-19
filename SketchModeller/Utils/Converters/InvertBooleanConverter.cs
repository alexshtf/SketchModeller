using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace Utils.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBooleanConverter : IValueConverter
    {
        public static readonly InvertBooleanConverter Instance = new InvertBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                Trace.WriteLine("value is not boolean");
                return Binding.DoNothing;
            }

            bool flag = (bool)value;
            return !flag;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                Trace.WriteLine("value is not boolean");
                return Binding.DoNothing;
            }

            bool flag = (bool)value;
            return !flag;
        }
    }
}
