using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Utils
{
    public class DelegateConverter<T> : IValueConverter
    {
        private readonly Func<T, object> converter;

        public DelegateConverter(Func<T, object> converter)
        {
            this.converter = converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var concrete = (T)value;
            return converter(concrete);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
