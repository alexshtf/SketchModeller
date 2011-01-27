using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using Utils;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// A matrix made of terms
    /// </summary>
    [Serializable]
    public class TMatrix
    {
        private readonly TVec[] rows;

        public TMatrix(Term[,] terms)
        {
            Contract.Requires(terms != null);
            Contract.Requires(terms.GetLength(0) > 0);
            Contract.Requires(terms.GetLength(1) > 0);

            var rowsCount = terms.GetLength(0);
            var colsCount = terms.GetLength(1);

            rows = new TVec[rowsCount];
            Term[] currentRow = new Term[colsCount];
            for (int row = 0; row < rowsCount; ++row)
            {
                for (int col = 0; col < colsCount; ++col)
                    currentRow[col] = terms[row, col];
                rows[row] = new TVec(currentRow);
            }
        }

        public TMatrix(IEnumerable<TVec> rows)
        {
            Contract.Requires(rows != null);
            Contract.Requires(!rows.IsEmpty()); // we have rows
            Contract.Requires(Contract.ForAll(rows, row => row.Dimension == rows.First().Dimension)); // all rows have the same size

            this.rows = rows.ToArray();
        }

        public TMatrix(params TVec[] rows)
            : this(rows as IEnumerable<TVec>)
        {
        }

        public int RowsCount
        {
            get { return rows.Length; }
        }

        public int ColsCount
        {
            get { return rows[0].Dimension; }
        }

        public TMatrix GetMinor(int row, int col)
        {
            #region Contracts
            // valid matrix size
            Contract.Requires(RowsCount > 1);
            Contract.Requires(ColsCount > 1);

            // valid indices
            Contract.Requires(row >= 0 && row < RowsCount);
            Contract.Requires(col >= 0 && col < ColsCount);

            // a valid result that is one row and one column smaller
            Contract.Ensures(Contract.Result<TMatrix>() != null);
            Contract.Ensures(Contract.Result<TMatrix>().RowsCount == RowsCount - 1);
            Contract.Ensures(Contract.Result<TMatrix>().ColsCount == ColsCount - 1);
            #endregion

            var result = new Term[RowsCount - 1, ColsCount - 1];
            for (int resultRowIdx = 0; resultRowIdx < result.GetLength(0); ++resultRowIdx)
            {
                var inputRow = rows[resultRowIdx < row ? resultRowIdx : resultRowIdx + 1]; // we skip one row in the input
                for (int resultColIdx = 0; resultColIdx < result.GetLength(1); ++resultColIdx)
                {
                    var inputColIdx = resultColIdx < col ? resultColIdx : resultColIdx + 1; // we skip one column in the input
                    result[resultRowIdx, resultColIdx] = inputRow[inputColIdx];
                }
            }

            return new TMatrix(result);
        }

        public Term GetDeterminant()
        {
            Contract.Requires(RowsCount == ColsCount);

            if (RowsCount > 2) // General case
            {
                // get minor determinants (recursively)
                var minorDets = new Term[RowsCount];
                for (int i = 0; i < minorDets.Length; ++i)
                    minorDets[i] = GetMinor(i, 0).GetDeterminant();

                // negate every second determinant
                for (int i = 1; i < minorDets.Length; i += 2)
                    minorDets[i] = -1 * minorDets[i];

                return TermBuilder.Sum(minorDets);
            }
            else if (RowsCount == 2)
                return rows[0][0] * rows[1][1] - rows[1][0] * rows[0][1]; // 2D determinant
            else  // RowsCount == 1
                return rows[0][0];  // 1D determinant - simply the term value
        }
    }
}
