﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Xml.Serialization;

namespace SketchModeller.Infrastructure.Data
{
    [DebuggerDisplay("Count = {Points.Count}")]
    [DebuggerTypeProxy(typeof(PointsSequenceDebugView))]
    public class PointsSequence : NotificationObject
    {
        /// <summary>
        /// The collection of points. Please do NOT assign to this property. The assignment is only for
        /// xml serialization support.
        /// </summary>
        public ObservableCollection<Point> Points { get; set; }

        [ContractInvariantMethod]
        private void InvariantsMethod()
        {
            Contract.Invariant(Contract.ForAll(Points, pnt => pnt != null));
        }

        public PointsSequence()
        {
            Points = new ObservableCollection<Point>();
        }

        public PointsSequence(IEnumerable<Point> points)
        {
            Contract.Requires(points != null);
            Contract.Requires(Contract.ForAll(points, pnt => pnt != null));

            Points = new ObservableCollection<Point>(points);
        }

        #region IsSelected property

        private bool isSelecgted;

        [XmlIgnore]
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
                get { return sequence.Points.ToArray(); }
            }
        }
    }
}
