using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public static class NumArrayUtils
    {
        public static void ApplyFunc(this double[,] array, Func<double, double> elementFunc)
        {
            for (int i = 0; i < array.GetLength(0); ++i)
                for (int j = 0; j < array.GetLength(1); ++j)
                    array[i, j] = elementFunc(array[i, j]);
        }

        /// <summary>
        /// Computes a per-element binary function of two 2D arrays
        /// </summary>
        /// <param name="first">The first array</param>
        /// <param name="second">The second array</param>
        /// <param name="func">The binary function</param>
        /// <returns>The resulting 2D array of the binary function's results.</returns>
        public static double[,] BinaryFunc(double[,] first, double[,] second, Func<double, double, double> func)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Requires(func != null);
            Contract.Requires(first.GetLength(0) == second.GetLength(0));
            Contract.Requires(first.GetLength(1) == second.GetLength(1));

            var rowsCount = first.GetLength(0);
            var colsCount = second.GetLength(1);

            var result = new double[rowsCount, colsCount];
            for (int row = 0; row < rowsCount; ++row)
                for (int col = 0; col < colsCount; ++col)
                    result[row, col] = func(first[row, col], second[row, col]);

            return result;
        }

        /// <summary>
        /// Performs a neighborhood filter on a 2D array of values.
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="filter">The filtering method. See remarks for more info.</param>
        /// <param name="size">The neighborhood size</param>
        /// <returns>A filtered 2D array</returns>
        /// <remarks>
        /// <para>
        /// The filtering function receives four params (N, val) and returns a value. The parameters
        /// are:
        /// </para>
        /// <list type="bullet">
        /// <item>N - an array of neighborhood pixels. Each element is a tuple (row, col, value).</item>
        /// <item>val - The value of the current pixel</item>
        /// </list>
        /// </remarks>
        public static double[,] NeighborhoodFilter(
            this double[,] array, 
            Func<Tuple<int, int, double>[], int, int, double, double> filter, 
            int size = 1)
        {
            Contract.Requires(array != null);
            Contract.Requires(filter != null);
            Contract.Requires(size >= 1);
            Contract.Ensures(Contract.Result<double[,]>() != null);
            Contract.Ensures(Contract.Result<double[,]>().GetLength(0) == array.GetLength(0));
            Contract.Ensures(Contract.Result<double[,]>().GetLength(1) == array.GetLength(1));

            var rowsCount = array.GetLength(0);
            var colsCount = array.GetLength(1);

            var result = new double[rowsCount, colsCount];
            for (int row = 0; row < rowsCount; ++row)
            {
                for (int col = 0; col < colsCount; ++col)
                {
                    // construct a LINQ query of valid (in range) neighborhood pixels
                    var neighborhoodQuery =
                        from neighborRow in Enumerable.Range(row - size, 1 + 2 * size)
                        from neighborCol in Enumerable.Range(col - size, 2 + 2 * size)
                        where neighborRow >= 0 && neighborRow < rowsCount
                        where neighborCol >= 0 && neighborCol < colsCount
                        select Tuple.Create(neighborRow, neighborCol, array[neighborRow, neighborCol]);

                    // execute the query and store the pixels in an array
                    var neighborhood = neighborhoodQuery.ToArray();

                    // compute the filtered pixel value
                    result[row,col] = filter(neighborhood, row, col, array[row, col]);
                }
            }

            return result;
        }

        /// <summary>
        /// Transforms an input 2D array such that every element is the average of its neighborhood
        /// in the original array.
        /// </summary>
        /// <param name="array">The array to transform</param>
        /// <param name="size">The neighborhood size</param>
        /// <returns>The transformed array</returns>
        public static double[,] AverageFilter(this double[,] array, int size = 1)
        {
            return array.NeighborhoodFilter((neighborhood, row, col, value) =>
                {
                    var avg = neighborhood.Select(n => n.Item3).Average(); // item3 is the pixel value
                    return avg;
                }, size);
        }

        /// <summary>
        /// Transforms an input 2D array such that every element is the difference of its value and 
        /// the neighborhood average.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double[,] LaplacianFilter(this double[,] array, int size = 1)
        {
            return array.NeighborhoodFilter((neighborhood, row, col, value) =>
                {
                    var avg = 
                        neighborhood
                        .Where(n => n.Item1 != row && n.Item2 != col) // we take only the neighborhood, not the pixel itself
                        .Select(n => n.Item3) // item3 is the pixel value
                        .Average(); 

                    return value - avg;
                }, size);
        }
    }
}
