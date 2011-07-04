using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SketchModeller.Utilities.Debugging
{
    public static class ArrayCSV
    {
        public static string Convert(double[,] values)
        {
            var builder = new StringBuilder();
            var rows = values.GetLength(0);
            var cols = values.GetLength(1);
            foreach (var row in Enumerable.Range(0, rows))
            {
                foreach (var col in Enumerable.Range(0, cols))
                    builder.AppendFormat("{0}, ", values[row, col]);

                builder.Remove(builder.Length - 2, 2);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public static string Convert(Point[] points)
        {
            var array = new double[points.Length, 2];
            foreach (var idx in Enumerable.Range(0, points.Length))
            {
                array[idx, 0] = points[idx].X;
                array[idx, 1] = points[idx].Y;
            }
            return Convert(array);
        }

        public static string Convert(int[,] values)
        {
            var doubleArray = new double[values.GetLength(0), values.GetLength(1)];
            foreach (var i in Enumerable.Range(0, values.GetLength(0)))
                foreach (var j in Enumerable.Range(0, values.GetLength(1)))
                    doubleArray[i, j] = values[i, j];
            return Convert(doubleArray);
        }
    }
}
