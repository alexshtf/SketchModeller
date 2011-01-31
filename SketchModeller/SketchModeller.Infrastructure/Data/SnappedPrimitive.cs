using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class SnappedPrimitive : NotificationObject
    {
        /// <summary>
        /// The list of sketch curves this primitive is snapped to.
        /// </summary>
        public PointsSequence[] SnappedTo { get; set; }

        /// <summary>
        /// A list of snapped point sets that connect points on this snapped primitive to curves on the sketch
        /// </summary>
        public SnappedPointsSet[] SnappedPointsSets { get; set; }

        /// <summary>
        /// The optimization term that snaps this primitive to the sketch, regardless of the annotations.
        /// </summary>
        public Term DataTerm { get; set; }
    }
}
