using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class ArrayUtils
    {
        public static T[] Generate<T>(int count)
            where T : new()
        {
            var result = new T[count];
            for (int i = 0; i < count; ++i)
                result[i] = new T();
            return result;
        }
    }
}
