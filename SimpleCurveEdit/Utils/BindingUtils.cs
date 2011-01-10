using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Linq.Expressions;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace Utils
{
    /// <summary>
    /// Various utilities related to WPF bindings.
    /// </summary>
    public static class BindingUtils
    {
        /// <summary>
        /// Binds a dependency property of a dependency object to the specified source, source path and converter.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="prop">The binding target property.</param>
        /// <param name="path">The source path</param>
        /// <param name="source">The binding source object.</param>
        /// <param name="converter">An optional converter for the binding.</param>
        public static void Bind(this DependencyObject target, DependencyProperty prop, string path, object source, IValueConverter converter = null)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);

            var binding = new Binding
            {
                Path      = new PropertyPath(path),
                Source    = source,
                Converter = converter,
            };
            BindingOperations.SetBinding(target, prop, binding);
        }

        public static void Bind<T>(this DependencyObject target, DependencyProperty prop, Expression<Func<T>> expr, Func<T, object> converter = null)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);
            Contract.Requires(expr != null);

            var targetAndMember = expr.GetTargetAndMember();
            var bindingSource = targetAndMember.Item1;
            var member = targetAndMember.Item2;

            target.Bind(prop, member.Name, bindingSource, CreateConverter(converter));
        }

        public static void Bind<T1, T2>(this DependencyObject target, DependencyProperty prop, Expression<Func<T1>> expr1, Expression<Func<T1>> expr2, Func<T1, T2, object> converter)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);
            Contract.Requires(expr1 != null);
            Contract.Requires(expr2 != null);
            Contract.Requires(converter != null);

            Bind(target, prop, CreateConverter(converter), expr1, expr2);
        }

        public static void Bind<T1, T2, T3>(
            this DependencyObject target,
            DependencyProperty prop, 
            Expression<Func<T1>> expr1, 
            Expression<Func<T2>> expr2,
            Expression<Func<T3>> expr3,
            Func<T1, T2, T3, object> converter)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);
            Contract.Requires(expr1 != null);
            Contract.Requires(expr2 != null);
            Contract.Requires(expr3 != null);
            Contract.Requires(converter != null);

            Bind(target, prop, CreateConverter(converter), expr1, expr2, expr3);
        }

        private static void Bind(DependencyObject target, DependencyProperty prop, IMultiValueConverter converter, params System.Linq.Expressions.Expression[] expressions)
        {
            var multiBinding = new MultiBinding();
            multiBinding.Converter = converter;

            foreach (var expression in expressions)
            {
                var targetAndMember = expression.GetTargetAndMember();
                var bindingSource = targetAndMember.Item1;
                var member = targetAndMember.Item2;
                
                var binding = new Binding(member.Name);
                binding.Source = bindingSource;
                multiBinding.Bindings.Add(binding);
            }

            BindingOperations.SetBinding(target, prop, multiBinding);
        }

        private static DelegateConverter<T> CreateConverter<T>(Func<T, object> func)
        {
            return new DelegateConverter<T>(func);
        }

        private static DoubleDelegateConverter<T1, T2> CreateConverter<T1, T2>(Func<T1, T2, object> func)
        {
            return new DoubleDelegateConverter<T1, T2>(func);
        }

        private static TripleDelegateConverter<T1, T2, T3> CreateConverter<T1, T2, T3>(Func<T1, T2, T3, object> func)
        {
            return new TripleDelegateConverter<T1, T2, T3>(func);
        }

        #region TripleDelegateConverter class

        private class TripleDelegateConverter<T1, T2, T3> : IMultiValueConverter
        {
            private Func<T1, T2, T3, object> converter;

            public TripleDelegateConverter(Func<T1, T2, T3, object> converter)
            {
                this.converter = converter;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var v0 = (T1)values[0];
                var v1 = (T2)values[1];
                var v2 = (T3)values[2];

                return converter(v0, v1, v2);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        #endregion

        #region DoubleDelegateConverter class

        private class DoubleDelegateConverter<T1, T2> : IMultiValueConverter
        {
            private readonly Func<T1, T2, object> converter;

            public DoubleDelegateConverter(Func<T1, T2, object> converter)
            {
                this.converter = converter;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var v1 = (T1)values[0];
                var v2 = (T2)values[1];

                return converter(v1, v2);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }


        #endregion
    }
}
