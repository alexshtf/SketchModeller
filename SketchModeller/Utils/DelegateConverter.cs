using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Diagnostics;

namespace Utils
{
    /// <summary>
    /// Converts values, for one-way bindings, with a user-specified delegate.
    /// </summary>
    /// <typeparam name="T">Type of the source values.</typeparam>
    public class DelegateConverter<T> : IValueConverter
    {
        private readonly Func<T, object> converter;
        private readonly StackTrace creationTrace;

        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="converter">The delegate to user-specified conversion function.</param>
        public DelegateConverter(Func<T, object> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            this.converter = converter;
            creationTrace = new StackTrace(true); // for debugging purposes
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
