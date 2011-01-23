using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using SketchModeller.Infrastructure;
using System.Diagnostics;
using System.Windows.Media;

namespace SketchModeller.Modelling.Services.Snap
{
    class PointsConverter : IMultiValueConverter
    {
        public static readonly PointsConverter Instance = new PointsConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 7)
            {
                Trace.TraceWarning("Parameters count must be 7");
                return Binding.DoNothing;
            }

            var pts = values[0] as IEnumerable<SketchModeller.Infrastructure.Data.Point>;
            if (pts == null)
            {
                Trace.TraceWarning("First input value must be a valid list of data points");
                return Binding.DoNothing;
            }

            if (!values.Skip(1).All(x => x is double))
            {
                Trace.TraceWarning("Input values [1..6] must be double floating point values");
                return Binding.DoNothing;
            }

            var minx = (double)values[1];
            var maxx = (double)values[2];
            var miny = (double)values[3];
            var maxy = (double)values[4];
            var width = (double)values[5];
            var height = (double)values[6];

            var result =
                from p in pts
                let x = Scale(p.X, maxx, minx, width)
                let y = Scale(p.Y, maxy, miny, height)
                select new System.Windows.Point(x, y);

            return new PointCollection(result);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(ErrorMessages.ConvertBackNotSupported);
        }

        private double Scale(double val, double max, double min, double size)
        {
            return 0.8 * size * (val - min) / (max - min) + 0.1 * size;
        }
    }
}
