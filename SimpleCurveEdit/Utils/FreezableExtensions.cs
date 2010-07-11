using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Utils
{
    public static  class FreezableExtensions
    {
        public static T SafeClone<T>(this T freezable)
            where T : Freezable
        {
            return (T)freezable.Clone();
        }
    }
}
