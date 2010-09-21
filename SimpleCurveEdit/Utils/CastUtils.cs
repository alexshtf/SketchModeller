using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class CastUtils
    {
        /// <summary>
        /// Performs a type-safe action on an object, if it is of the specified type. Allows chaining such operations
        /// to perform a "pattern-match" like behavior.
        /// </summary>
        /// <typeparam name="T">The type of objects the action works on.</typeparam>
        /// <param name="o">The target object.</param>
        /// <param name="action">The action to perform.</param>
        /// <returns>A result object that allows chaining such operations. See remarks.</returns>
        /// <remarks>
        /// This method's purpose is to allow syntax similar to pattern matching in functional languages by testing the target object's 
        /// type. The typical usage is like this:
        /// <code>
        ///     obj.MatchClass&lt;Class1&gt;(c1 =>
        ///     {
        ///         // Do stuff with c1
        ///     }).MatchClass&lt;Class2&gt;(c2 =>
        ///     {
        ///         // do stuff with c2
        ///     }).MatchClass&lt;object&gt;(o =>
        ///     {
        ///         // Notify about an error for unexpected type.
        ///     });
        /// </code>
        /// The above code will check if obj is of type <c>Class1</c> and perform the first lambda action. If it is not, it will
        /// test if it is of type <c>Class2</c> and if it is - perform the second lambda action. Otherwise, it will check 
        /// if the object is of type <c>object</c> (every object is) and will notify about a type error.
        /// </remarks>
        public static MatchClassChain MatchClass<T>(this object o, Action<T> action)
            where T : class
        {
            Contract.Requires(action != null);

            T concrete = o as T;
            if (concrete != null)
            {
                action(concrete);
                return new MatchClassChain(o, true);
            }
            else
                return new MatchClassChain(o, false);
        }

        public static void MatchStruct<T>(object o, Action<T> action)
            where T : struct
        {
            Contract.Requires(action != null);

            if (o is T)
            {
                T concrete = (T)o;
                action(concrete);
            }
        }

        public class MatchClassChain
        {
            private readonly object obj;
            private readonly bool castSuccess;

            internal MatchClassChain(object obj, bool castSuccess)
            {
                this.obj = obj;
                this.castSuccess = castSuccess;
            }

            public MatchClassChain DoWithClass<T>(Action<T> action)
                where T : class
            {
                if (!castSuccess)
                    return obj.MatchClass(action);
                else
                    return this;
            }
        }
    }
}
