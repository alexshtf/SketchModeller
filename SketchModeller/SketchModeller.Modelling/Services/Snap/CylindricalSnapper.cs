using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using Utils;
using SketchModeller.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.Snap
{
    abstract class CylindricalSnapper<TNew, TSnapped> : BasePrimitivesSnapper<TNew, TSnapped>
        where TNew : NewCylindricalPrimitive
        where TSnapped : SnappedCylindricalPrimitive, new()
    {
        protected override TSnapped Create(PointsSequence[] selectedCurves, TNew newPrimitive)
        {
            var snappedCone = InitNewSnapped(newPrimitive);

            var allSequences = selectedCurves;
            snappedCone.SnappedTo = selectedCurves;

            var features = allSequences.Where(x => x.CurveCategory == CurveCategories.Feature).ToArray();
            var silhouettes = allSequences.Except(features).ToArray();

            var topPts = features.Where(x => SnapperHelper.IsTop(x, snappedCone)).ToArray();
            var botPts = features.Except(topPts).ToArray();

            Debug.Assert(topPts.Length <= 1, "The algorithm cannot handle more than one top curve");
            Debug.Assert(botPts.Length <= 1, "The algorithm cannot handle more than one bottom curve");

            snappedCone.TopCurve = topPts.FirstOrDefault();
            snappedCone.BottomCurve = botPts.FirstOrDefault();
            snappedCone.Silhouettes = silhouettes;

            var pointsSets = new List<SnappedPointsSet>();
            if (topPts.Length > 0)
                pointsSets.Add(
                    new SnappedPointsSet(
                        snappedCone.GetTopCenter(),
                        snappedCone.Axis,
                        GetTopRadius(snappedCone),
                        topPts[0]));
            if (botPts.Length > 0)
                pointsSets.Add(
                    new SnappedPointsSet(
                        snappedCone.BottomCenter,
                        snappedCone.Axis,
                        GetBottomRadius(snappedCone),
                        botPts[0]));
            snappedCone.SnappedPointsSets = pointsSets.ToArray();

            return snappedCone;
        }

        protected override Tuple<Term, Term[]> Reconstruct(TSnapped snappedPrimitive)
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

        private Tuple<Term, Term[]> TwoSilhouettesNoFeatures(TSnapped snappedPrimitive)
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
                GetTopRadius(snappedPrimitive),
                new Polyline());
            var botPointsSet = new SnappedPointsSet(
                snappedPrimitive.BottomCenter,
                snappedPrimitive.Axis,
                GetBottomRadius(snappedPrimitive),
                new Polyline());

            var topFit = ProjectionFit(topPointsSet, new Point[] { sil0Top, sil1Top });
            var botFit = ProjectionFit(botPointsSet, new Point[] { sil0Bot, sil1Bot });

            var objective = TermUtils.SafeSum(topFit.Concat(botFit));
            var constraints = new Term[] { snappedPrimitive.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }

        private Tuple<Term, Term[]> TwoSilhouettesSingleFeature(TSnapped snappedPrimitive)
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
            Term radius = null;
            if (snappedPrimitive.TopCurve == null)
            {
                center = snappedPrimitive.GetTopCenter();
                radius = GetTopRadius(snappedPrimitive);
            }
            else if (snappedPrimitive.BottomCurve == null)
            {
                center = snappedPrimitive.BottomCenter;
                radius = GetBottomRadius(snappedPrimitive);
            }
            Debug.Assert(center != null);
            Debug.Assert(radius != null);

            var farPointsSet = new SnappedPointsSet(
                center,
                snappedPrimitive.Axis,
                radius,
                new Polyline());
            var farProj = ProjectionFit(farPointsSet, new Point[] { sil0Far, sil1Far });

            var objective = TermUtils.SafeSum(featureProj.Concat(farProj));
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

        private Point GetFarPoint(Tuple<Point, Point> segment, PointsSequence curve)
        {
            var p1Dist = curve.Points.Min(p => (p - segment.Item1).LengthSquared);
            var p2Dist = curve.Points.Min(p => (p - segment.Item2).LengthSquared);

            if (p1Dist > p2Dist)
                return segment.Item1;
            else
                return segment.Item2;
        }

        private Tuple<Term, Term[]> FullInfo(TSnapped snappedPrimitive)
        {
            var terms =
                from item in snappedPrimitive.SnappedPointsSets
                from term in ProjectionFit(item)
                select term;

            var objective = TermUtils.SafeAvg(terms);
            var constraints = new Term[] { snappedPrimitive.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }

        private TSnapped InitNewSnapped(TNew newPrimitive)
        {
            TSnapped snapped = new TSnapped();
            snapped.Axis = SnapperHelper.GenerateVarVector();
            snapped.BottomCenter = SnapperHelper.GenerateVarVector();
            snapped.Length = new Variable();

            snapped.AxisResult = newPrimitive.Axis.Normalized();
            snapped.BottomCenterResult = newPrimitive.Bottom;
            snapped.LengthResult = newPrimitive.Length;

            SpecificInit(newPrimitive, snapped);

            return snapped;
        }



        protected abstract void SpecificInit(TNew newPrimitive, TSnapped snapped);
        protected abstract Term GetTopRadius(TSnapped snapped);
        protected abstract Term GetBottomRadius(TSnapped snapped);
    }
}
