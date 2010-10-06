using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;

namespace Utils
{
    /// <summary>
    /// Extension methods for <see cref="Freezable"/> types.
    /// </summary>
    public static  class FreezableExtensions
    {
        /// <summary>
        /// Clones a freezable object in a type-safe manner (casting it back to its type after cloning).
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="freezable">The freezable object to clone.</param>
        /// <returns>A clone of <paramref name="freezable"/>.</returns>
        public static T CloneSafely<T>(this T freezable)
            where T : Freezable
        {
            Contract.Requires(freezable != null);

            return (T)freezable.Clone();
        }
    }
}
