using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.Snap
{
    class GeometricTests
    {
        public static Term DiffSquared(TVec[] left, TVec[] right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Length == right.Length);
            Contract.Requires(Contract.ForAll(left, item => item != null));
            Contract.Requires(Contract.ForAll(right, item => item != null));
            Contract.Requires(Contract.ForAll(0, left.Length, i => left[i].Dimension == right[i].Dimension));

            var leftFlat = left.SelectMany(x => x.GetTerms()).ToArray();
            var rightFlat = right.SelectMany(x => x.GetTerms()).ToArray();

            return TermUtils.DiffSquared(leftFlat, rightFlat);
        }

        /// <summary>
        /// A term that is close to zero when the points are close to being coplanar.
        /// </summary>
        /// <param name="vecs">The input points.</param>
        /// <returns>A term representing the points' co-planarity measure.</returns>
        [Pure]
        public static Term Coplanarity3D(params TVec[] vecs)
        {
            Contract.Requires(Contract.ForAll(vecs, vec => vec != null));
            Contract.Requires(Contract.ForAll(vecs, vec => vec.Dimension == 3));
            Contract.Requires(vecs.Length == 4);

            var rows = vecs.Select(vec => new TVec(vec, 1));
            var mat = new TMatrix(rows);

            return mat.GetDeterminant();
        }

        /// <summary>
        /// A term that is close to zero when the input points all lie on the same sphere.
        /// </summary>
        /// <param name="vecs">The set of input points</param>
        /// <returns>The term representing the co-sphericality measure.</returns>
        [Pure]
        public static Term Cosphericality3D(params TVec[] vecs)
        {
            Contract.Requires(Contract.ForAll(vecs, vec => vec != null));
            Contract.Requires(Contract.ForAll(vecs, vec => vec.Dimension == 3));
            Contract.Requires(vecs.Length == 5);

            var rows = vecs.Select(vec => new TVec(vec.NormSquared, vec[0], vec[1], vec[2], 1));
            var mat = new TMatrix(rows);

            return mat.GetDeterminant();
        }

        /// <summary>
        /// Performs a geometric test of sub-sequences of a points sequence.
        /// </summary>
        /// <param name="pts">The input points sequence</param>
        /// <param name="subsequenceSize">The size of the chosen sub-sequences.</param>
        /// <param name="subsequenceTest">A test function that given a sub-sequence, returns a measure of how well the subsequence
        /// has the wanted property.</param>
        /// <returns>A term representing the squared-norm of all subsequence test results.</returns>
        [Pure]
        public static Term SubsequencesTest(TVec[] pts, int subsequenceSize, [Pure] Func<TVec[], Term> subsequenceTest)
        {
            Contract.Requires(pts != null);
            Contract.Requires(pts.Length >= subsequenceSize);

            var testResults = new Term[pts.Length - subsequenceSize + 1];
            var subArray = new TVec[subsequenceSize];
            for (int i = 0; i < testResults.Length; ++i)
            {
                Array.Copy(pts, i, subArray, 0, subsequenceSize);
                testResults[i] = subsequenceTest(subArray);
            }

            return new TVec(testResults).NormSquared;
        }

        [Pure]
        public static Term MultiCoplanarity3D(TVec[] pts)
        {
            Contract.Requires(Contract.ForAll(pts, pnt => pnt.Dimension == 3));
            return SubsequencesTest(pts, 4, Coplanarity3D);
        }

        [Pure]
        public static Term MultiCosphericality3D(TVec[] pts)
        {
            Contract.Requires(Contract.ForAll(pts, pnt => pnt.Dimension == 3));
            return SubsequencesTest(pts, 5, Cosphericality3D);
        }
    }
}
