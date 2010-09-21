using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Utils
{
    /// <summary>
    /// A base class for view models. Supports property change notification via strings and via lambda expressions
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event with a string-based property name.
        /// </summary>
        /// <param name="propertyName">The name of the changed property or <see cref="string.Empty"/> if all properties
        /// have changed.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Fires the <see cref="PropertyChanged"/> event with a lambda-based expression. The expression must be 
        /// a lambda expression of getting the desired property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property. This type parameter should be inferred.</typeparam>
        /// <param name="expression">A lambda-expression with the property getter.</param>
        /// <example>
        /// The code <c>NotifyPropertyChanged(() => MyProperty)</c> will fire the property changed event notifying that the
        /// property "MyProperty" has changed.</example>
        /// <remarks>
        /// This method is preferred to the string-based property changed notification because of smaller chance of mistakes 
        /// in property names and is updated automatically when the property name is changed with a refactoring command. The main
        /// drawback is performance. Notification using a string is much cheaper.</remarks>
        protected void NotifyPropertyChanged<TProperty>(Expression<Func<TProperty>> expression)
        {
            NotifyPropertyChanged(expression.GetMemberInfo().Name);
        }
    }
}
