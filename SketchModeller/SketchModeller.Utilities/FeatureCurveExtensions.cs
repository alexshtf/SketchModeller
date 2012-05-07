using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace SketchModeller.Infrastructure.Data
{
    public static class FeatureCurveExtensions
    {
        /// <summary>
        /// Checks if a feature curve is free, that is the opposite of snapped.
        /// </summary>
        /// <param name="featureCurve">The feature curve to check</param>
        /// <returns>The opposite of the <see cref="FeatureCurve.IsSnapped"/> method</returns>
        public static bool IsFree(this FeatureCurve featureCurve)
        {
            Contract.Requires(featureCurve != null);
            return !featureCurve.IsSnapped();
        }

        /// <summary>
        /// Checks weather two feature curves actually represent the same curve on the sketch (by 
        /// checking which sketch curves they are snapped to).
        /// </summary>
        /// <param name="left">The first curve</param>
        /// <param name="right">The second curve</param>
        /// <returns><c>true</c> if and only if both <paramref name="left"/> and <paramref name="right"/>
        /// curves are snapped to the same object curve on the sketch.</returns>
        public static bool IsSameObjectCurve(this FeatureCurve left, FeatureCurve right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left is CircleFeatureCurve);
            Contract.Requires(right is CircleFeatureCurve);

            var leftCircle = left as CircleFeatureCurve;
            var rightCircle = right as CircleFeatureCurve;

            return leftCircle.SnappedTo != null 
                && rightCircle.SnappedTo != null 
                && leftCircle.SnappedTo == rightCircle.SnappedTo;
        }
    }
}
