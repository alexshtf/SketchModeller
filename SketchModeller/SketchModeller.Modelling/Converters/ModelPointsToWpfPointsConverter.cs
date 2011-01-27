using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using System.Windows.Media;
using SketchModeller.Infrastructure;
using System.Globalization;
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.Converters
{
    [ValueConversion(typeof(IEnumerable<Point>), typeof(PointCollection))]
    class ModelPointsToWpfPointsConverter : IValueConverter
    {
        public static readonly ModelPointsToWpfPointsConverter Instance = new ModelPointsToWpfPointsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<Point>))
            {
                Trace.TraceError("value is not of the correct type");
                return Binding.DoNothing;
            }

            var enumerable = value as IEnumerable<Point>;
            return new PointCollection(enumerable);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ErrorMessages.ConvertBackNotSupported);
        }
    }
}
