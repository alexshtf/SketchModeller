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
        public SnappedPointsSet(TVec center, TVec axis, Term radius, PointsSequence snappedTo)
        {
            Contract.Requires(center != null);
            Contract.Requires(axis != null);
            Contract.Requires(radius != null);
            Contract.Requires(snappedTo != null);

            Center = center;
            Axis = axis;
            Radius = radius;
            SnappedTo = snappedTo;
        }

        public TVec Center { get; private set; }
        public TVec Axis { get; private set; }
        public Term Radius { get; private set; }
        public PointsSequence SnappedTo { get; private set; }
    }
}
