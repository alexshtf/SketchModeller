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
        /// A collection of this primitive's feature curves. 
        /// </summary>
        public FeatureCurve[] FeatureCurves { get; set; }

        /// <summary>
        /// The optimization term that snaps this primitive to the sketch, regardless of the annotations.
        /// </summary>
        public Term DataTerm { get; set; }

        #region IsMarked property

        [NonSerialized]
        private bool isMarked;

        public bool IsMarked
        {
            get { return isMarked; }
            set
            {
                isMarked = value;
                RaisePropertyChanged(() => IsMarked);
            }
        }

        #endregion

        public abstract void UpdateFeatureCurves();
    }
}
