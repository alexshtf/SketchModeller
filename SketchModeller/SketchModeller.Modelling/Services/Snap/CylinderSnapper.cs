using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using Utils;
using System.Diagnostics;
using SketchModeller.Utilities;
using System.Windows;

namespace SketchModeller.Modelling.Services.Snap
{
    class CylinderSnapper : BasePrimitivesSnapper<NewCylinder, SnappedCylinder>
    {
        protected override SnappedCylinder Create(PointsSequence[] selectedCurves, NewCylinder newPrimitive)
        {
            var snappedCylinder = InitNewSnapped(newPrimitive);

            var allSequences = selectedCurves;
            snappedCylinder.SnappedTo = allSequences;

            var features = allSequences.Where(x => x.CurveCategory == CurveCategories.Feature).ToArray();
            var silhouettes = allSequences.Except(features).ToArray();

            var topPts = features.Where(x => SnapperHelper.IsTop(x, snappedCylinder)).ToArray();
            var botPts = features.Except(topPts).ToArray();

            Debug.Assert(topPts.Length <= 1, "The algorithm cannot handle more than one top curve");
            Debug.Assert(botPts.Length <= 1, "The algorithm cannot handle more than one bottom curve");

            snappedCylinder.TopCurve = topPts.FirstOrDefault();
            snappedCylinder.BottomCurve = botPts.FirstOrDefault();
            snappedCylinder.Silhouettes = silhouettes;

            var pointsSets = new List<SnappedPointsSet>();
            if (topPts.Length > 0)
                pointsSets.Add(
                    new SnappedPointsSet(
                        snappedCylinder.GetTopCenter(),
                        snappedCylinder.Axis,
                        snappedCylinder.Radius,
                        topPts[0]));
            if (botPts.Length > 0)
                pointsSets.Add(
                    new SnappedPointsSet(
                        snappedCylinder.BottomCenter,
                        snappedCylinder.Axis,
                        snappedCylinder.Radius,
                        botPts[0]));
            snappedCylinder.SnappedPointsSets = pointsSets.ToArray();

            return snappedCylinder;
        }

        protected override Tuple<Term, Term[]> Reconstruct(SnappedCylinder snappedPrimitive)
        {
            var topCurve = snappedPrimitive.TopCurve;
            var botCurve = snappedPrimitive.BottomCurve;
            var silhouettes = snappedPrimitive.Silhouettes;

            // simple case - we use the two curves to reconstruct the cylinder
            if (topCurve != null && botCurve != null)
                return FullInfo(snappedPrimitive);
            if (topCurve == null && botCurve != null && silhouettes.Length == 2)
                return TwoSilhouettesBottomCurve(snappedPrimitive);

            throw new NotImplementedException("Not implemented all missing info yet");
        }

        private Tuple<Term, Term[]> TwoSilhouettesBottomCurve(SnappedCylinder snappedPrimitive)
        {

            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[0].Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[1].Points);

            var sil0Far = GetFarPoint(sil0, snappedPrimitive.BottomCurve);
            var sil1Far = GetFarPoint(sil1, snappedPrimitive.BottomCurve);

            // we have only the bottom curve - so it's the only projection fit
            var bottomProj = ProjectionFit(snappedPrimitive.SnappedPointsSets[0]);

            // create an imaginary snapped points set with the top curve's data
            var topPointSet = new SnappedPointsSet(
                snappedPrimitive.GetTopCenter(),
                snappedPrimitive.Axis,
                snappedPrimitive.Radius, 
                new Polyline());
            var topProj = ProjectionFit(topPointSet, new Point[] { sil0Far, sil1Far });

            var objective = TermUtils.SafeSum(topProj.Concat(bottomProj));
            var constraints = new Term[] { snappedPrimitive.Axis.NormSquared - 1 };
            return Tuple.Create(objective, constraints);
        }

        private Point GetFarPoint(Tuple<Point, Point> segment, PointsSequence curve)
        {
            var p1Dist = curve.Points.Min(p => (p - segment.Item1).LengthSquared);
            var p2Dist = curve.Points.Min(p => (p - segment.Item2).LengthSquared);

            if (p1Dist > p2Dist)
                return segment.Item1;
            else
                return segment.Item2;
        }

        private Tuple<Term, Term[]> FullInfo(SnappedCylinder cylinder)
        {
            var terms =
                from item in cylinder.SnappedPointsSets
                from term in ProjectionFit(item)
                select term;

            var objective = TermUtils.SafeAvg(terms);
            var constraints = new Term[] { cylinder.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }

        private static SnappedCylinder InitNewSnapped(NewCylinder selectedCylinder)
        {
            var snappedCylinder = new SnappedCylinder();
            snappedCylinder.Axis = SnapperHelper.GenerateVarVector();
            snappedCylinder.BottomCenter = SnapperHelper.GenerateVarVector();
            snappedCylinder.Length = new Variable();
            snappedCylinder.Radius = new Variable();

            snappedCylinder.AxisResult = selectedCylinder.Axis.Normalized();
            snappedCylinder.BottomCenterResult = selectedCylinder.Bottom;
            snappedCylinder.RadiusResult = selectedCylinder.Radius;
            snappedCylinder.LengthResult = selectedCylinder.Length;
            return snappedCylinder;
        }
    }
}
