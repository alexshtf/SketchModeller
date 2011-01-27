using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

using System.Globalization;
using SketchModeller.Infrastructure;
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.Converters
{
    [ValueConversion(typeof(IEnumerable<Point>), typeof(Geometry))]
    class ModelPointsToScatterConverter : IValueConverter
    {
        public static readonly ModelPointsToScatterConverter Instance = new ModelPointsToScatterConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<Point>))
            {
                Trace.TraceError("value is not of the correct type");
                return Binding.DoNothing;
            }

            var enumerable = value as IEnumerable<Point>;
            var result = new GeometryGroup();
            foreach (var point in enumerable)
            {
                var ellipseGeometry = new EllipseGeometry(point, 0.5, 0.5);
                result.Children.Add(ellipseGeometry);
            }

            result.Freeze();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ErrorMessages.ConvertBackNotSupported);
        }
    }
}
