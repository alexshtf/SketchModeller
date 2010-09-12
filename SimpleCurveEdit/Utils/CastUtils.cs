using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class CastUtils
    {
        public static object DoWithClass<T>(this object o, Action<T> action)
            where T : class
        {
            Contract.Requires(action != null);

            T concrete = o as T;
            if (concrete != null)
            {
                action(concrete);
                return null;
            }
            else
                return o;
        }

        public static void DoWithStruct<T>(object o, Action<T> action)
            where T : struct
        {
            Contract.Requires(action != null);

            if (o is T)
            {
                T concrete = (T)o;
                action(concrete);
            }
        }
    }
}
