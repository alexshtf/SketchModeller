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
using Enumerable = System.Linq.Enumerable;
using TermUtils = SketchModeller.Utilities.TermUtils;
using SketchModeller.Modelling.Computations;

namespace SketchModeller.Modelling.Services.Snap
{
    abstract class CylindricalSnapper<TNew, TSnapped> : BasePrimitivesSnapper<TNew, TSnapped>
        where TNew : NewCylindricalPrimitive
        where TSnapped : SnappedCylindricalPrimitive, new()
    {
        protected override TSnapped Create(PointsSequence[] selectedCurves, TNew newPrimitive)
        {
            var snappedPrimitive = InitNewSnapped(newPrimitive);
            snappedPrimitive.SnappedTo = 
                newPrimitive.AllCurves
                .Select(c => c.AssignedTo)
                .Where(c => c != null)
                .ToArray();

            snappedPrimitive.TopFeatureCurve.SnappedTo 
                = newPrimitive.TopCircle.AssignedTo;

            snappedPrimitive.BottomFeatureCurve.SnappedTo 
                = newPrimitive.BottomCircle.AssignedTo;

            snappedPrimitive.LeftSilhouette = newPrimitive.LeftSilhouette.AssignedTo;
            snappedPrimitive.RightSilhouette = newPrimitive.RightSilhouette.AssignedTo;

            return snappedPrimitive;
        }

        protected override Tuple<Term, Term[]> Reconstruct(TSnapped snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            var topCurve = snappedPrimitive.TopFeatureCurve.SnappedTo;
            var botCurve = snappedPrimitive.BottomFeatureCurve.SnappedTo;;
            var silhouettes = new PointsSequence[] { snappedPrimitive.LeftSilhouette, snappedPrimitive.RightSilhouette };

            // get annotated feature curves of this primitive.
            var annotated = new HashSet<FeatureCurve>(curvesToAnnotations.Keys.Where(key => curvesToAnnotations[key].Count > 0));
            annotated.Intersect(snappedPrimitive.FeatureCurves);

            Tuple<Term, Term[]> result = null;
            if (topCurve != null && botCurve != null)
                // simple case - we use the two curves to reconstruct the cylinder
                result = FullInfo(snappedPrimitive);
            else if (silhouettes.Length == 2)
            {
                if (!(topCurve == null && botCurve == null))
                    result = TwoSilhouettesSingleFeature(snappedPrimitive, annotated);
                else
                    result = TwoSilhouettesNoFeatures(snappedPrimitive, annotated);
            }

            if (result != null)
                return IncorporateRadiusConstraint(snappedPrimitive, result);
            else
                throw new NotImplementedException("Not implemented all missing info yet");
        }

        #region abstract methods

        protected abstract void SpecificInit(TNew newPrimitive, TSnapped snapped);
        protected abstract Term GetTopRadius(TSnapped snapped);
        protected abstract Term GetBottomRadius(TSnapped snapped);
        protected abstract Term GetRadiusSoftConstraint(TSnapped snapped, double expectedTop, double expectedBottom);

        #endregion

        private Tuple<Term, Term[]> IncorporateRadiusConstraint(TSnapped snappedPrimitive, Tuple<Term, Term[]> result)
        {
            if (snappedPrimitive.LeftSilhouette != null && snappedPrimitive.RightSilhouette != null)
            {
                var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.LeftSilhouette.Points);
                var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.RightSilhouette.Points);
                var axis2d = Get2DVector(snappedPrimitive.AxisResult);

                var sil0Top = GetForwardPoint(sil0, axis2d);
                var sil0Bot = GetForwardPoint(sil0, -axis2d);
                var sil1Top = GetForwardPoint(sil1, axis2d);
                var sil1Bot = GetForwardPoint(sil1, -axis2d);

                var expectedTop = 0.5 * Math.Min(
                    sil0Top.DistanceFromSegment(sil1.Item1, sil1.Item2),
                    sil1Top.DistanceFromSegment(sil0.Item1, sil0.Item2));
                var expectedBottom = 0.5 * Math.Min(
                    sil0Bot.DistanceFromSegment(sil1.Item1, sil1.Item2),
                    sil1Bot.DistanceFromSegment(sil0.Item1, sil0.Item2));

                var radiusTerm = GetRadiusSoftConstraint(snappedPrimitive, expectedTop, expectedBottom);
                var newTargetFunc = 0.95 * radiusTerm + 0.05 * result.Item1;

                return Tuple.Create(newTargetFunc, result.Item2);
            }
            else
                return result;
        }

        private Tuple<Term, Term[]> TwoSilhouettesNoFeatures(TSnapped snappedPrimitive, ISet<FeatureCurve> annotated)
        {
            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.LeftSilhouette.Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.RightSilhouette.Points);
            var axis2d = Get2DVector(snappedPrimitive.AxisResult);

            var sil0Top = GetForwardPoint(sil0, axis2d);
            var sil0Bot = GetForwardPoint(sil0, -axis2d);
            var sil1Top = GetForwardPoint(sil1, axis2d);
            var sil1Bot = GetForwardPoint(sil1, -axis2d);

            var topFit = ProjectionFit.Compute(
                snappedPrimitive.TopFeatureCurve, new Point[] { sil0Top, sil1Top });
            var botFit = ProjectionFit.Compute(
                snappedPrimitive.BottomFeatureCurve, new Point[] { sil0Bot, sil1Bot });

            if (annotated.Contains(snappedPrimitive.TopFeatureCurve))
                topFit = Enumerable.Empty<Term>();
            if (annotated.Contains(snappedPrimitive.BottomFeatureCurve))
                botFit = Enumerable.Empty<Term>();

            var objective = TermUtils.SafeSum(topFit.Concat(botFit));
            var constraints = new Term[] { snappedPrimitive.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }

        private Tuple<Term, Term[]> TwoSilhouettesSingleFeature(TSnapped snappedPrimitive, ISet<FeatureCurve> annotated)
        {
            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.LeftSilhouette.Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.RightSilhouette.Points);
            var axis2d = Get2DVector(snappedPrimitive.AxisResult);

            var isTopSnapped = snappedPrimitive.TopFeatureCurve.SnappedTo != null;
            var isBottomSnapped = snappedPrimitive.BottomFeatureCurve.SnappedTo != null;

            var snappedFeatureCurve = isTopSnapped ? snappedPrimitive.TopFeatureCurve : snappedPrimitive.BottomFeatureCurve;
            var unsnappedFeatureCurve = isTopSnapped ? snappedPrimitive.BottomFeatureCurve : snappedPrimitive.TopFeatureCurve;

            var sil0Top = GetForwardPoint(sil0, axis2d);
            var sil0Bot = GetForwardPoint(sil0, -axis2d);
            var sil1Top = GetForwardPoint(sil1, axis2d);
            var sil1Bot = GetForwardPoint(sil1, -axis2d);

            var sil0Far = isTopSnapped ? sil0Bot : sil0Top;
            var sil1Far = isTopSnapped ? sil1Bot : sil1Top;

            var featureProj = ProjectionFit.Compute(snappedFeatureCurve);
            var farProj = Enumerable.Repeat(EndpointsProjectionFit(unsnappedFeatureCurve, sil0Far, sil1Far), 1);

            if (annotated.Contains(unsnappedFeatureCurve))
                farProj = Enumerable.Empty<Term>();

            var objective = TermUtils.SafeSum(new Term[] { TermUtils.SafeAvg(featureProj), TermUtils.SafeAvg(farProj) });
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
                from item in snappedPrimitive.FeatureCurves.Cast<CircleFeatureCurve>()
                from term in ProjectionFit.Compute(item)
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

        protected Term EndpointsProjectionFit(CircleFeatureCurve pointsSet, Point p1, Point p2)
        {
            var x1 = p1.X;
            var y1 = -p1.Y;
            
            var x2 = p2.X;
            var y2 = -p2.Y;

            var cx = pointsSet.Center.X;
            var cy = pointsSet.Center.Y;
            var r = pointsSet.Radius;

            var eq1 = TermBuilder.Power(x1 - cx, 2) + TermBuilder.Power(y1 - cy, 2) - TermBuilder.Power(r, 2);
            var eq2 = TermBuilder.Power(x2 - cx, 2) + TermBuilder.Power(y2 - cy, 2) - TermBuilder.Power(r, 2);

            var result = TermBuilder.Power(eq1, 2) + TermBuilder.Power(eq2, 2);

            return result;
        }
    }
}
