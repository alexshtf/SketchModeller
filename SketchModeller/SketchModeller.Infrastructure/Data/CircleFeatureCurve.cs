using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// A feature curve that is a planar circle.
    /// </summary>
    [Serializable]
    public class CircleFeatureCurve : FeatureCurve
    {
        /// <summary>
        /// A term for the circle's radius
        /// </summary>
        public Term Radius { get; set; }

        /// <summary>
        /// The result of the last radius computation
        /// </summary>
        public double RadiusResult { get; set; }

        /// <summary>
        /// The sketch curve that this feature curve is snapped to.
        /// </summary>
        public PointsSequence SnappedTo { get; set; }

        public override bool IsSnapped()
        {
            return SnappedTo != null;
        }
    }
}
