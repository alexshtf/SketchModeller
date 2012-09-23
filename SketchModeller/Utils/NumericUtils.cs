using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Utils
{
    public static class NumericUtils
    {
        public static readonly double DBL_MACHINE_EPSILON = Math.Pow(2, -52);

        /// <summary>
        /// Measures "almost equality" using relative accuracy and machine-epsilon.
        /// </summary>
        /// <param name="value">The value we got from our calculation</param>
        /// <param name="reference">The reference value to test against</param>
        /// <param name="epsilonScale">The machine-epsilon multiplier</param>
        /// <returns><c>true</c> if the relative error of <paramref name="value"/> against <paramref name="reference"/>
        /// is less than <paramref name="epsilonScale"/> times the machine epsilon.</returns>
        [Pure]
        public static bool AlmostEqual(double value, double reference, double epsilonScale)
        {
            Contract.Requires(epsilonScale >= 1);

            return Math.Abs(value - reference) / Math.Abs(reference) <= epsilonScale * DBL_MACHINE_EPSILON;
        }

        /// <summary>
        /// Measures relative equality using maximum fracton (percentage) difference.
        /// </summary>
        /// <param name="value">The value to compare to the reference</param>
        /// <param name="reference">The reference value</param>
        /// <param name="maxFraction">The maximum fractional difference within which the values are considered equal</param>
        /// <returns><c>true</c> if and only if <paramref name="value"/> divided by <paramref name="reference"/> is within the range
        /// <c>[1 - maxFraction, 1 + maxFraction]</c>.</returns>
        [Pure]
        public static bool AreRelativeClose(double value, double reference, double maxFraction)
        {
            Contract.Requires(maxFraction > 0);

            var ratio = Math.Abs(value) / Math.Abs(reference);
            if (ratio > 1 + maxFraction)
                return false;
            if (ratio < 1 - maxFraction)
                return false;

            return true;
        }
    }
}
