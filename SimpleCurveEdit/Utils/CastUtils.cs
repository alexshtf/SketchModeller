using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class CastUtils
    {
        public static DoWithClassHelper DoWithClass<T>(this object o, Action<T> action)
            where T : class
        {
            Contract.Requires(action != null);

            T concrete = o as T;
            if (concrete != null)
            {
                action(concrete);
                return new DoWithClassHelper(o, true);
            }
            else
                return new DoWithClassHelper(o, false);
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

        public class DoWithClassHelper
        {
            private readonly object obj;
            private readonly bool castSuccess;

            internal DoWithClassHelper(object obj, bool castSuccess)
            {
                this.obj = obj;
                this.castSuccess = castSuccess;
            }

            public DoWithClassHelper DoWithClass<T>(Action<T> action)
                where T : class
            {
                if (!castSuccess)
                    return obj.DoWithClass(action);
                else
                    return this;
            }
        }
    }
}
