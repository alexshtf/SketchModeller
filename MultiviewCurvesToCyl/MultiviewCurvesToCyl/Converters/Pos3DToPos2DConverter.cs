using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Media3D;
using System.Windows;

namespace MultiviewCurvesToCyl.Converters
{
    [ValueConversion(typeof(Point3D), typeof(Point))]
    class Pos3DToPos2DConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point3D)
            {
                var concrete = (Point3D)value;
                return new Point(concrete.X, concrete.Y);
            }
            else
                return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("One way converters don't support ConvertBack.");
        }
    }
}
