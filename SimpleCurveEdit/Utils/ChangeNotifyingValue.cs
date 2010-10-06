using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    /// A class that notifies when a value really changes. The user can assign a value multiple times,
    /// and the event will be fired when the newly assigned value is different from the previous one.
    /// </summary>
    /// <typeparam name="T">The type of the valus.</typeparam>
    public class ChangeNotifyingValue<T>
        where T : IEquatable<T>
    {
        private static readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default;
        private T storedValue;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get { return storedValue; }
            set
            {
                if (!Comparer.Equals(storedValue, value))
                {
                    storedValue = value;
                    if (ValueChanged != null)
                        ValueChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when <see cref="Value"/> actually changes when it's set.
        /// </summary>
        public event EventHandler ValueChanged;
    }
}
