using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class ArrayUtils
    {
        public static T[] Flatten<T>(this T[,] array)
        {
            Contract.Requires(array != null);
            Contract.Ensures(Contract.Result<T[]>() != null);
            Contract.Ensures(Contract.Result<T[]>().Length == array.GetLength(0) * array.GetLength(1));

            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var result = new T[rows * cols];

            int idx = 0;
            for (int row = 0; row < rows; ++row)
                for (int col = 0; col < cols; ++col)
                    result[idx++] = array[row, col];

            return result;
        }

        public static T[] Generate<T>(int count)
            where T : new()
        {
            var result = new T[count];
            for (int i = 0; i < count; ++i)
                result[i] = new T();
            return result;
        }

        public static void RotateRight<T>(T[] array)
        {
            Array.Reverse(array, 0, array.Length - 1);
            Array.Reverse(array);
        }
    }
}
