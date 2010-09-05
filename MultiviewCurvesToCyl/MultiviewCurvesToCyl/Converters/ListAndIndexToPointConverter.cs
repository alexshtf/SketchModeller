using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace MultiviewCurvesToCyl.Converters
{
    class ListAndIndexToPointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // validate that the converter has two values to convert.
            if (values.Length != 2)
                return Binding.DoNothing;

            // validate that converter parameter is a valid string
            var parameterStr = parameter as string;
            if (string.IsNullOrEmpty(parameterStr))
                return Binding.DoNothing;

            // validate the content of the parameter string.
            parameterStr = parameterStr.ToUpper();
            if (parameterStr != "X" && parameterStr != "Y")
                return Binding.DoNothing;

            // validate types of the values to convert.
            if (values[0] is IEnumerable<Point> && values[1] is int)
            {
                var pnts = values[0] as IEnumerable<Point>;
                var idx = (int)values[1];
                var pnt = pnts.ElementAt(idx);

                if (parameterStr == "X")
                    return pnt.X;
                else // we know that it is "Y" because of the validation above.
                    return pnt.Y;
            }
            else
                return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported");
        }
    }
}
