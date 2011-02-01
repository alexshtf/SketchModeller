using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Windows;

namespace SketchModeller.Infrastructure.Data
{
    [DebuggerDisplay("Count = {Points.Count}")]
    [DebuggerTypeProxy(typeof(PointsSequenceDebugView))]
    [Serializable]
    public class PointsSequence : NotificationObject
    {
        /// <summary>
        /// The collection of points. Please do NOT assign to this property. The assignment is only for
        /// xml serialization support.
        /// </summary>
        public Point[] Points { get; set; }

        public PointsSequence(IEnumerable<Point> points = null)
        {
            if (points != null)
                Points = points.ToArray();
        }

        #region CurveCategory property

        private CurveCategories curveCategory;

        public CurveCategories CurveCategory
        {
            get { return curveCategory; }
            set
            {
                curveCategory = value;
                RaisePropertyChanged(() => CurveCategory);
            }
        }

        #endregion

        #region IsSelected property

        [NonSerialized]
        private bool isSelecgted;

        public bool IsSelected
        {
            get { return isSelecgted; }
            set
            {
                isSelecgted = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        #endregion

        internal class PointsSequenceDebugView
        {
            private PointsSequence sequence;

            public PointsSequenceDebugView(PointsSequence sequence)
            {
                this.sequence = sequence;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Point[] Points
            {
                get 
                {
                    return sequence.Points;
                }
            }
        }
    }
}
