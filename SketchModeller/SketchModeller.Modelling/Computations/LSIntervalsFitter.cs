using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Meta.Numerics.Matrices;

namespace SketchModeller.Modelling.Computations
{
    /// <summary>
    /// Fits a set of 3rd degree polynomials to a set of values using Least-Squares fitting. 
    /// </summary>
    static class LSIntervalsFitter
    {
        /// <summary>
        /// Fits a set of 3rd degree polynomials given a set of values, using Least-Squares fitting. 
        /// </summary>
        /// <param name="values">An array of values used to fit the polynomial functions.</param>
        /// <param name="breakIndices">The indices where the values array is broken into intervals.</param>
        /// <returns>An array of coefficients for each interval. Each interval is itself an array of 4 coefficients of the polynomial.</returns>
        /// <remarks>
        /// <para>
        /// We assume that the array of values is a sampled function f:R -> R, such that f(i) is given by values[i]. We define n = values.Length. 
        /// The integer interval [0, n) is divided to subintervals such that:
        /// </para>
        /// <list type="bullet">
        /// <item>Each sub-interval contains at-least 4 points.</item>
        /// <item>If i is the last point of a sub-interval then i is the first point of the next sub-interval, unless i = n - 1</item>
        /// <item>Each sub-interval contains at-least 4 points.</item>
        /// <item>The union of all sub-intervals is [0, n)</item>
        /// </list>
        /// <para>The array <paramref name="breakIndices"/> defines the breaking points of [0, n) into sub-intervals. That is, breakIndices[j] is the
        /// last point of the j-th interval.
        /// </para>
        /// <para>
        /// The method fits one polynomial for each sub-interval and returns its coefficients. The fitting is done such that:
        /// </para>
        /// <list type="bullet">
        /// <item>The mean squared error between each polynomial and the points in its interval is minimized</item>
        /// <item>For any two intervals that share a point i, the polynomials corresponding to the intervals give the same value for i</item>
        /// <item>For any two intervals that share a point i, the polynomials corresponding to the intervals give the same 1-st derivative value for i</item>
        /// <item>For any two intervals that share a point i, the polynomials corresponding to the intervals give the same 2-nd derivative value for i</item>
        /// </list>
        /// <para>
        /// The above requirements assure us that the sequence of polynomials is a smooth function that fits the values.
        /// </para>
        /// </remarks>
        public static double[][] FitIntervals(double[] values, params int[] breakIndices)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length >= 4);
            Contract.Requires(breakIndices != null);
            Contract.Requires(breakIndices.Length >= 1); // we have at-least one interval
            Contract.Requires(Contract.ForAll(0, breakIndices.Length - 1, i => breakIndices[i + 1] - breakIndices[i] >= 3)); // each interval has at-least four points

            Contract.Ensures(Contract.Result<double[][]>() != null);
            Contract.Ensures(Contract.Result<double[][]>().Length == breakIndices.Length); // one polynomial for every interval
            Contract.Ensures(Contract.ForAll(Contract.Result<double[][]>(), coefficients => coefficients != null && coefficients.Length == 4)); // we have four coefficients for each interval

            var intervalsCount = breakIndices.Length;

            // build linear system we get by using Lagrange multipliers on the optimization problem
            SquareMatrix leftHandMatrix;
            ColumnVector rightHandVector;
            BuildLinearSystem(values, breakIndices, intervalsCount, out leftHandMatrix, out rightHandVector);

            // solve the linear system to get the primal and dual values
            var solution = SolveLinearSystem(leftHandMatrix, rightHandVector);

            // create the result using the primal values (5*i, ..., 5*i + 3 are the coefficients for interval i)
            var result = new double[intervalsCount][];
            for (int i = 0; i < intervalsCount; ++i)
            {
                result[i] = new double[4];
                for (int j = 0; j < 4; ++j)
                    result[i][j] = solution[7 * i + j];
            }

            return result;
        }

        private static ColumnVector SolveLinearSystem(SquareMatrix leftHandMatrix, ColumnVector rightHandVector)
        {
            var lu = leftHandMatrix.LUDecomposition();
            var solution = lu.Solve(rightHandVector);
            return solution;
        }

        private static void BuildLinearSystem(double[] values, int[] breakIndices, int intervalsCount, out SquareMatrix leftHandMatrix, out ColumnVector rightHandVector)
        {
            // build linear system of equations to solve the approximation problem
            var systemSize = 7 * intervalsCount - 3; // matrix size
            leftHandMatrix = new SquareMatrix(systemSize);
            rightHandVector = new ColumnVector(systemSize);
            for (int intervalIndex = 0; intervalIndex < intervalsCount; ++intervalIndex)
            {
                var intervalStart = intervalIndex == 0 ? 0 : breakIndices[intervalIndex - 1];
                var intervalEnd = breakIndices[intervalIndex];
                var pointsCount = intervalEnd - intervalStart + 1;
                var baseMatrixIndex = 7 * intervalIndex;
                var ts = Enumerable.Range(intervalStart, pointsCount);

                // fill the 4x4 part of the matrix
                foreach (var row in Enumerable.Range(baseMatrixIndex, 4))
                {
                    foreach (var col in Enumerable.Range(baseMatrixIndex, 4))
                    {
                        var sum = ts.Select(t => Math.Pow(t, row + col - 2 * baseMatrixIndex)).Sum();
                        leftHandMatrix[row, col] = sum;
                    }
                    rightHandVector[row] = ts.Sum(t => values[t] * Math.Pow(t, row - baseMatrixIndex));
                }

                if (intervalIndex < intervalsCount - 1) // we add additional matrix elements for non-last intervals
                {
                    // fill the 5th row and column of the matrix
                    foreach (var i in Enumerable.Range(baseMatrixIndex + 1, 3))
                    {
                        var relativeIdx = i - baseMatrixIndex;
                        var value = relativeIdx * Math.Pow(ts.Last(), relativeIdx - 1);
                        leftHandMatrix[i, baseMatrixIndex + 4] = value;
                        leftHandMatrix[i + 7, baseMatrixIndex + 4] = -value;
                        leftHandMatrix[baseMatrixIndex + 4, i] = value;
                        leftHandMatrix[baseMatrixIndex + 4, i + 7] = -value;
                    }

                    // fill the 6th row and column of the matrix
                    foreach (var i in Enumerable.Range(baseMatrixIndex, 4))
                    {
                        var relativeIdx = i - baseMatrixIndex;
                        var value = Math.Pow(ts.Last(), relativeIdx);
                        leftHandMatrix[i, baseMatrixIndex + 5] = value;
                        leftHandMatrix[i + 7, baseMatrixIndex + 5] = -value;
                        leftHandMatrix[baseMatrixIndex + 5, i] = value;
                        leftHandMatrix[baseMatrixIndex + 5, i + 7] = -value;
                    }

                    {
                        // fill the 7th row and column of the matrix
                        var i = baseMatrixIndex + 2;
                        var value = 2;
                        leftHandMatrix[i, baseMatrixIndex + 6] = value;
                        leftHandMatrix[i + 7, baseMatrixIndex + 6] = -value;
                        leftHandMatrix[baseMatrixIndex + 6, i] = value;
                        leftHandMatrix[baseMatrixIndex + 6, i + 7] = -value;

                        i = baseMatrixIndex + 3;
                        value = 6 * ts.Last();
                        leftHandMatrix[i, baseMatrixIndex + 6] = value;
                        leftHandMatrix[i + 7, baseMatrixIndex + 6] = -value;
                        leftHandMatrix[baseMatrixIndex + 6, i] = value;
                        leftHandMatrix[baseMatrixIndex + 6, i + 7] = -value;
                    }
                }
            }
        }
    }
}
