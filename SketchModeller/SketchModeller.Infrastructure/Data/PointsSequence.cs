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
    [DebuggerDisplay("Count = {Points.Length}")]
    [DebuggerTypeProxy(typeof(PointsSequenceDebugView))]
    [Serializable]
    public class PointsSequence : NotificationObject
    {
        public const int INVALID_COLOR_CODING = -1;

        /// <summary>
        /// The collection of points. Please do NOT assign to this property. The assignment is only for
        /// xml serialization support.
        /// </summary>
        public Point[] Points { get; set; }
        public bool isdeselected = false; 
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

        #region ColorCodingIndex property

        [NonSerialized]
        private int colorCodingIndex = INVALID_COLOR_CODING;

        public int ColorCodingIndex
        {
            get { return colorCodingIndex; }
            set
            {
                colorCodingIndex = value;
                RaisePropertyChanged(() => ColorCodingIndex);
            }
        }

        #endregion

        #region IsEmphasized property

        [NonSerialized]
        private bool isEmphasized;

        public bool IsEmphasized
        {
            get { return isEmphasized; }
            set
            {
                isEmphasized = value;
                RaisePropertyChanged(() => IsEmphasized);
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
