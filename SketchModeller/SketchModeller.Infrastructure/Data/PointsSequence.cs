using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace SketchModeller.Infrastructure.Data
{
    [DebuggerDisplay("Count = {Points.Count}")]
    [DebuggerTypeProxy(typeof(PointsSequenceDebugView))]
    public class PointsSequence : NotificationObject
    {
        public ObservableCollection<Point> Points { get; private set; }

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
