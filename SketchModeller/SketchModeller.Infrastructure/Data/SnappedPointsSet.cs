using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Utils;

namespace SketchModeller.Infrastructure.Data
{
    [Serializable]
    public class SnappedPointsSet
    {
        public SnappedPointsSet(IEnumerable<TVec> pointTerms, PointsSequence snappedTo)
        {
            Contract.Requires(pointTerms != null);
            Contract.Requires(!pointTerms.IsEmpty());
            Contract.Requires(Contract.ForAll(pointTerms, t => t != null));
            Contract.Requires(snappedTo != null);

            Contract.Ensures(PointTerms.Count == pointTerms.Count());
            Contract.Ensures(SnappedTo == snappedTo);
            Contract.Ensures(Contract.ForAll(pointTerms, t => PointTerms.Contains(t)));

            PointTerms = Array.AsReadOnly(pointTerms.ToArray());
            SnappedTo = snappedTo;
        }

        public ReadOnlyCollection<TVec> PointTerms { get; private set; }
        public PointsSequence SnappedTo { get; private set; }
    }
}
