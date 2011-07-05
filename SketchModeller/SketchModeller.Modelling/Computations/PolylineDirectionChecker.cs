using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Computations
{
    static class PolylineDirectionChecker
    {
        public static bool AreSameDirection(Point[] l1, Point[] l2)
        {
            var p1 = l1.Concat(l2).ToArray();
            var p2 = l1.Concat(l2.Reverse()).ToArray();

            var ma1 = TwoLinesMedialAxis.Compute(l1, l2, p1);
            var ma2 = TwoLinesMedialAxis.Compute(l1, l2, p2);

            var length1 = ma1.Length >= 2 ? PointsSequenceExtensions.ComputeCurveLength(ma1) : 0;
            var length2 = ma2.Length >= 2 ? PointsSequenceExtensions.ComputeCurveLength(ma2) : 0;

            return length2 > length1;
        }

        private static double GetProximityDistance(Point[] l1, Point[] l2)
        {
            var d1 = ProximityDistanceEstimate.Compute(l1);
            var d2 = ProximityDistanceEstimate.Compute(l2);
            return Math.Min(d1, d2);
        }
    }
}
