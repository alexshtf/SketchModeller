using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace MultiviewCurvesToCyl.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    class ScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Binding.DoNothing;

            if (value == null)
                return Binding.DoNothing;

            try
            {
                var scale = System.Convert.ToDouble(parameter);
                var input = System.Convert.ToDouble(value);

                return scale * input;
            }
            catch (FormatException)
            {
                Trace.TraceError("FormatException occured");
                return Binding.DoNothing;
            }
            catch (InvalidCastException)
            {
                Trace.TraceError("InvalidCastException occured");
                return Binding.DoNothing;
            }
            catch (OverflowException)
            {
                Trace.TraceError("Overflow exception occured");
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Binding.DoNothing;

            if (value == null)
                return Binding.DoNothing;

            try
            {
                var scale = System.Convert.ToDouble(parameter);
                if (scale == 0)
                {
                    Trace.TraceWarning("Scale is zero. Cannot convert back");
                    return Binding.DoNothing;
                }

                var input = System.Convert.ToDouble(value);
                return input / scale;
            }
            catch (FormatException)
            {
                Trace.TraceError("FormatException occured");
                return Binding.DoNothing;
            }
            catch (InvalidCastException)
            {
                Trace.TraceError("InvalidCastException occured");
                return Binding.DoNothing;
            }
            catch (OverflowException)
            {
                Trace.TraceError("Overflow exception occured");
                return Binding.DoNothing;
            }
        }
    }
}
