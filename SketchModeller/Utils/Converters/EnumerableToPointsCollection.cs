using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

namespace Utils.Converters
{
    [ValueConversion(typeof(IEnumerable<Point>), typeof(PointCollection))]
    public class EnumerableToPointsCollection  :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var enumerable = value as IEnumerable<Point>;
            PointCollection result = null;
            if (enumerable != null)
                result = new PointCollection(enumerable);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack should never be called");
        }
    }
}
