using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using AutoDiff;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public abstract class SnappedPrimitive : SelectablePrimitive
    {
        /// <summary>
        /// The list of sketch curves this primitive is snapped to.
        /// </summary>
        public PointsSequence[] SnappedTo { get; set; }

        /// <summary>
        /// A collection of this primitive's feature curves. 
        /// </summary>
        public FeatureCurve[] FeatureCurves { get; set; }

        public abstract void UpdateFeatureCurves();
    }
}
