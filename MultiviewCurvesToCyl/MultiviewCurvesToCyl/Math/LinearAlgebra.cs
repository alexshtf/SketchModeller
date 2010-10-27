using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics.Matrices;
using System.Diagnostics.Contracts;

namespace MultiviewCurvesToCyl
{
    class LinearAlgebra
    {
        /// <summary>
        /// Finds the least norm solution to the system of linear equation Ax = b
        /// </summary>
        /// <param name="matrix">The matrix A of the system</param>
        /// <param name="vec">The vector b of the system.</param>
        /// <returns>The least norm solution of the system Ax = b</returns>
        public static ColumnVector LeastNormSolution(Matrix matrix, ColumnVector vec)
        {
            Contract.Requires(matrix != null);
            Contract.Requires(vec != null);
            Contract.Requires(matrix.RowCount == vec.Dimension);

            var transpose = matrix.Transpose();

            // we will build a big system that will solve the problem, according to lagrange
            // multipliers:
            // (I A^t)(x)   (0)
            // (     )    = ( )
            // (A  0 )(u)   (b)
            var bigMatrixSize = matrix.RowCount + matrix.ColumnCount;
            var bigMatrix = new SquareMatrix(bigMatrixSize);

            // embed the identity matrix at the top-left corner
            var identitySize = matrix.ColumnCount;
            for (int i = 0; i < identitySize; ++i)
                bigMatrix[i, i] = 1;

            // embed the transpose in the top-right corner
            for (int r = 0; r < transpose.RowCount; ++r)
                for (int c = identitySize; c < bigMatrixSize; ++c)
                    bigMatrix[r, c] = transpose[r, c - identitySize];

            // embed he matrix in the bottom-left corner
            for (int r = identitySize; r < bigMatrixSize; ++r)
                for (int c = 0; c < matrix.ColumnCount; ++c)
                    bigMatrix[r, c] = matrix[r - identitySize, c];

            // create big vector
            var bigVector = Enumerable.Repeat(0.0, matrix.ColumnCount).Concat(vec).ToArray();

            // solve the system
            var lu = bigMatrix.LUDecomposition();
            var solution = lu.Solve(bigVector);

            // extract the result
            var resultValues =
                solution.Take(matrix.ColumnCount).ToList();
            return new ColumnVector(resultValues);
        }
    }
}
