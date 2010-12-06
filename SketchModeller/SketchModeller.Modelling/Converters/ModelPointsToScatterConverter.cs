using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

using ModelPoint = SketchModeller.Infrastructure.Data.Point;
using System.Globalization;
using SketchModeller.Infrastructure;
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.Converters
{
    [ValueConversion(typeof(IEnumerable<ModelPoint>), typeof(Geometry))]
    class ModelPointsToScatterConverter : IValueConverter
    {
        public static readonly ModelPointsToScatterConverter Instance = new ModelPointsToScatterConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<ModelPoint>))
            {
                Trace.TraceError("value is not of the correct type");
                return Binding.DoNothing;
            }

            var enumerable = value as IEnumerable<ModelPoint>;
            var result = new GeometryGroup();
            foreach (var point in enumerable)
            {
                var ellipseGeometry = new EllipseGeometry(new Point(point.X, point.Y), 0.5, 0.5);
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
