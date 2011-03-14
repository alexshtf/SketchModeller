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
using System.Windows.Media.Media3D;

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

            if (topCurve != null && botCurve != null)
                // simple case - we use the two curves to reconstruct the cylinder
                return FullInfo(snappedPrimitive);
            else if (silhouettes.Length == 2)
            {
                if (!(topCurve == null && botCurve == null))
                    return TwoSilhouettesSingleFeature(snappedPrimitive);
                else
                    return TwoSilhouettesNoFeatures(snappedPrimitive);
            }

            throw new NotImplementedException("Not implemented all missing info yet");
        }

        private Tuple<Term, Term[]> TwoSilhouettesNoFeatures(SnappedCylinder snappedPrimitive)
        {
            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[0].Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[1].Points);
            var axis2d = Get2DVector(snappedPrimitive.AxisResult);

            var sil0Top = GetForwardPoint(sil0, axis2d);
            var sil0Bot = GetForwardPoint(sil0, -axis2d);
            var sil1Top = GetForwardPoint(sil1, axis2d);
            var sil1Bot = GetForwardPoint(sil1, -axis2d);

            var topPointsSet = new SnappedPointsSet(
                snappedPrimitive.GetTopCenter(),
                snappedPrimitive.Axis,
                snappedPrimitive.Radius,
                new Polyline());
            var botPointsSet = new SnappedPointsSet(
                snappedPrimitive.BottomCenter,
                snappedPrimitive.Axis,
                snappedPrimitive.Radius,
                new Polyline());

            var topFit = ProjectionFit(topPointsSet, new Point[] { sil0Top, sil1Top });
            var botFit = ProjectionFit(botPointsSet, new Point[] { sil0Bot, sil1Bot });

            var objective = TermUtils.SafeSum(topFit.Concat(botFit));
            var constraints = new Term[] { snappedPrimitive.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }

        private Point GetForwardPoint(Tuple<Point, Point> silPoints, Vector axis)
        {
            var silVec = silPoints.Item2 - silPoints.Item1;
            if (axis * silVec > 0)
                return silPoints.Item2;
            else
                return silPoints.Item1;
        }

        private Vector Get2DVector(Vector3D vector3D)
        {
            var p = UiState.SketchPlane.Center;
            var q = p + vector3D;

            var pp = UiState.SketchPlane.Project(p);
            var qq = UiState.SketchPlane.Project(q);
            return qq - pp;
        }

        private Tuple<Term, Term[]> TwoSilhouettesSingleFeature(SnappedCylinder snappedPrimitive)
        {
            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[0].Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[1].Points);

            var featureCurve = snappedPrimitive.SnappedPointsSets[0].SnappedTo;
            var sil0Far = GetFarPoint(sil0, featureCurve);
            var sil1Far = GetFarPoint(sil1, featureCurve);

            // we have only one feature curve - so it's the only feature projection fit
            var featureProj = ProjectionFit(snappedPrimitive.SnappedPointsSets[0]);

            // create an imaginary points set for the missing feature curve
            // we will fit it to the two far silhouette points
            TVec center = null;
            if (snappedPrimitive.TopCurve == null)
                center = snappedPrimitive.GetTopCenter();
            else if (snappedPrimitive.BottomCurve == null)
                center = snappedPrimitive.BottomCenter;
            Debug.Assert(center != null);
         
            var farPointsSet = new SnappedPointsSet(
                center,
                snappedPrimitive.Axis,
                snappedPrimitive.Radius,
                new Polyline());
            var farProj = ProjectionFit(farPointsSet, new Point[] { sil0Far, sil1Far });

            var objective = TermUtils.SafeSum(featureProj.Concat(farProj));
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
