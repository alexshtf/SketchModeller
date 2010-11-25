using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using ModelPoint = SketchModeller.Infrastructure.Data.Point;
using System.Windows.Media;
using SketchModeller.Infrastructure;
using System.Globalization;
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.Converters
{
    [ValueConversion(typeof(IEnumerable<ModelPoint>), typeof(PointCollection))]
    class ModelPointsToWpfPointsConverter : IValueConverter
    {
        public static readonly ModelPointsToWpfPointsConverter Instance = new ModelPointsToWpfPointsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<ModelPoint>))
            {
                Trace.TraceError("value is not of the correct type");
                return Binding.DoNothing;
            }

            var enumerable = value as IEnumerable<ModelPoint>;
            return new PointCollection(enumerable.Select(x => new Point(x.X, x.Y)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ErrorMessages.ConvertBackNotSupported);
        }
    }
}
