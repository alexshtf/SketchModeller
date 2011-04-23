using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Media.Media3D;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// A planar feature curve of a 3D primitive. Used to constrain the relationships between the different 
    /// primitives.
    /// </summary>
    [Serializable]
    public class FeatureCurve : NotificationObject
    {
        /// <summary>
        /// The center of mass term for this feature curve. Must not be <c>null</c>.
        /// </summary>
        public TVec Center { get; set; }

        /// <summary>
        /// The normal term for this feature curve. Must not be <c>null</c>.
        /// </summary>
        public TVec Normal { get; set; }

        /// <summary>
        /// The last result for the center of mass computation
        /// </summary>
        public Point3D CenterResult { get; set; }

        /// <summary>
        /// The last result for the normal computation.
        /// </summary>
        public Vector3D NormalResult { get; set; }

        /// <summary>
        /// The sketch curve that this feature curve is snapped to.
        /// </summary>
        public PointsSequence SnappedTo { get; set; }

        #region IsSelected property

        [NonSerialized]
        private bool fieldName;

        /// <summary>
        /// Tells weather this feature curve is currently selected by the user.
        /// </summary>
        public bool IsSelected
        {
            get { return fieldName; }
            set
            {
                fieldName = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        #endregion
    }
}
