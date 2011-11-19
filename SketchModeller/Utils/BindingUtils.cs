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

            target.Bind(prop, new PropertyPath(path), source, converter);
        }

        /// <summary>
        /// Binds a dependency property of a dependency object to the specified source, source path and converter.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="prop">The binding target property.</param>
        /// <param name="path">The source path</param>
        /// <param name="source">The binding source object.</param>
        /// <param name="converter">An optional converter for the binding.</param>
        public static void Bind(this DependencyObject target, DependencyProperty prop, PropertyPath path, object source, IValueConverter converter = null)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);

            var binding = new Binding
            {
                Path = path,
                Source = source,
                Converter = converter,
            };
            BindingOperations.SetBinding(target, prop, binding);
        }

        /// <summary>
        /// Binds a dependency property of a dependency object to a source object and property specified using type-safe lambda syntax.
        /// </summary>
        /// <typeparam name="T">Type of the source property</typeparam>
        /// <param name="target">The binding target</param>
        /// <param name="prop">The target property</param>
        /// <param name="expr">Lambda expression specifying the binding source and property.</param>
        /// <param name="converter">A converter delegate from the source value to the target value.</param>
        /// <remarks>The expression <paramref name="expr"/> must be of the form <c>() => source.Property</c></remarks>
        /// <example>
        /// <para>
        /// The following code will bind the <c>Text</c> property of a <c>TextBlock</c>.
        /// <code>
        /// TextBlock textBlock;
        /// textBlock.Bind(
        ///     TextBlock.TextProperty, 
        ///     () => viewModel.SomeText); // viewModel is the binding source. SomeText is the path
        /// </code>
        /// The following code will bind the <c>Text</c> property of a <c>TextBlock</c> with a converter.
        /// <code>
        /// Textblock textBlock;
        /// textBlock.Bind(
        ///     TextBlock.TextProperty, 
        ///     () => viewModel.SomeData, // viewModel is the binding source. SomeData is the path.
        ///     data => data.ToString()); // The binding converter will convert the data to a string.
        /// </code>
        /// </para>
        /// </example>
        public static void Bind<T>(this DependencyObject target, DependencyProperty prop, Expression<Func<T>> expr, Func<T, object> converter = null)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);
            Contract.Requires(expr != null);

            // a null converter means identity converter
            if (converter == null)
                converter = x => x;

            var targetAndMember = expr.GetTargetAndMember();
            var bindingSource = targetAndMember.Item1;
            var member = targetAndMember.Item2;

            target.Bind(prop, member.Name, bindingSource, CreateConverter(converter));
        }
        
        /// <summary>
        /// Binds a dependency property of a dependency object to two sources using type-safe lambda syntax.
        /// </summary>
        /// <typeparam name="T1">Type of the first source property</typeparam>
        /// <typeparam name="T2">Type of the second source property</typeparam>
        /// <param name="target">The binding target</param>
        /// <param name="prop">The target property</param>
        /// <param name="expr1">Lambda expression specifying the first binding source.</param>
        /// <param name="expr2">Lambda expression specifying the second binding source.</param>
        /// <param name="converter">A converter delegate to convert two source values to the target value.</param>
        /// <remarks>The expressions <paramref name="expr1"/> and <paramref name="expr2"/> must be of the form <c>() => source.Property</c></remarks>
        /// <seealso cref="Bind{T}"/>
        public static void Bind<T1, T2>(this DependencyObject target, DependencyProperty prop, Expression<Func<T1>> expr1, Expression<Func<T2>> expr2, Func<T1, T2, object> converter)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);
            Contract.Requires(expr1 != null);
            Contract.Requires(expr2 != null);
            Contract.Requires(converter != null);

            Bind(target, prop, CreateConverter(converter), expr1, expr2);
        }

        /// <summary>
        /// Binds a dependency property of a dependency object to three source using type-safe lambda syntax.
        /// </summary>
        /// <typeparam name="T1">The type of the first source property</typeparam>
        /// <typeparam name="T2">The type of the second source property</typeparam>
        /// <typeparam name="T3">The type of the third source property</typeparam>
        /// <param name="target">The binding target</param>
        /// <param name="prop">The target property</param>
        /// <param name="expr1">Lambda expression specifying the first binding source</param>
        /// <param name="expr2">Lambda expression specifying the second binding source</param>
        /// <param name="expr3">Lambda expression specifying the third binding source</param>
        /// <param name="converter">A converter delegate to convert the three source values to the target value</param>
        /// <remarks>The expressions <paramref name="expr1"/>, <paramref name="expr2"/> and <paramref name="expr3"/> must be of the form <c>() => source.Property</c></remarks>
        /// <seealso cref="Bind{T}"/>
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

        /// <summary>
        /// Binds a dependency property of a dependency object to three source using type-safe lambda syntax.
        /// </summary>
        /// <typeparam name="T1">The type of the first source property</typeparam>
        /// <typeparam name="T2">The type of the second source property</typeparam>
        /// <typeparam name="T3">The type of the third source property</typeparam>
        /// <typeparam name="T4">The type of the fourth source property</typeparam>
        /// <param name="target">The binding target</param>
        /// <param name="prop">The target property</param>
        /// <param name="expr1">Lambda expression specifying the first binding source</param>
        /// <param name="expr2">Lambda expression specifying the second binding source</param>
        /// <param name="expr3">Lambda expression specifying the third binding source</param>
        /// <param name="expr4">Lambda expression specifying the fourth binding source</param>
        /// <param name="converter">A converter delegate to convert the three source values to the target value</param>
        /// <remarks>The expressions <paramref name="expr1"/>, <paramref name="expr2"/> and <paramref name="expr3"/> must be of the form <c>() => source.Property</c></remarks>
        /// <seealso cref="Bind{T}"/>
        public static void Bind<T1, T2, T3, T4>(
            this DependencyObject target,
            DependencyProperty prop,
            Expression<Func<T1>> expr1,
            Expression<Func<T2>> expr2,
            Expression<Func<T3>> expr3,
            Expression<Func<T4>> expr4,
            Func<T1, T2, T3, T4, object> converter)
        {
            Contract.Requires(target != null);
            Contract.Requires(prop != null);
            Contract.Requires(expr1 != null);
            Contract.Requires(expr2 != null);
            Contract.Requires(expr3 != null);
            Contract.Requires(expr4 != null);
            Contract.Requires(converter != null);

            Bind(target, prop, CreateConverter(converter), expr1, expr2, expr3, expr4);
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

        private static QuadDelegateConverter<T1, T2, T3, T4> CreateConverter<T1, T2, T3, T4>(Func<T1, T2, T3, T4, object> func)
        {
            return new QuadDelegateConverter<T1, T2, T3, T4>(func);
        }

        #region QuadDelegateConverter class

        private class QuadDelegateConverter<T1, T2, T3, T4> : IMultiValueConverter
        {
            private readonly Func<T1, T2, T3, T4, object> converter;

            public QuadDelegateConverter(Func<T1, T2, T3, T4, object> converter)
            {
                this.converter = converter;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                var v1 = (T1)values[0];
                var v2 = (T2)values[1];
                var v3 = (T3)values[2];
                var v4 = (T4)values[3];

                return converter(v1, v2, v3, v4);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        #endregion

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
