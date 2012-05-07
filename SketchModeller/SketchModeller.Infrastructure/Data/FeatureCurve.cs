using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using Microsoft.Practices.Prism.ViewModel;
using System.Windows.Media.Media3D;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    /// <summary>
    /// A planar feature curve of a 3D primitive. Used to constrain the relationships between the different 
    /// primitives.
    /// </summary>
    [Serializable]
    public abstract class FeatureCurve : NotificationObject
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

        /// <summary>
        /// Checks if this feature curve is snapped to the sketch, or is "free"
        /// </summary>
        /// <returns><c>true</c> if and only if this feature curve is snapped to the sketch</returns>
        public abstract bool IsSnapped();
    }


}
