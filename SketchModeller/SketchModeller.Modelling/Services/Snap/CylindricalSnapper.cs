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

            if (topPts.Length > 0)
                snappedCone.TopFeatureCurve.SnappedTo = topPts[0];
            if (botPts.Length > 0)
                snappedCone.BottomFeatureCurve.SnappedTo = botPts[0];

            return snappedCone;
        }

        protected override Tuple<Term, Term[]> Reconstruct(TSnapped snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            var topCurve = snappedPrimitive.TopCurve;
            var botCurve = snappedPrimitive.BottomCurve;
            var silhouettes = snappedPrimitive.Silhouettes;

            // get annotated feature curves of this primitive.
            var annotated = new HashSet<FeatureCurve>(curvesToAnnotations.Keys.Where(key => curvesToAnnotations[key].Count > 0));
            annotated.Intersect(snappedPrimitive.FeatureCurves);

            if (topCurve != null && botCurve != null)
                // simple case - we use the two curves to reconstruct the cylinder
                return FullInfo(snappedPrimitive);
            else if (silhouettes.Length == 2)
            {
                if (!(topCurve == null && botCurve == null))
                    return TwoSilhouettesSingleFeature(snappedPrimitive, annotated);
                else
                    return TwoSilhouettesNoFeatures(snappedPrimitive, annotated);
            }

            throw new NotImplementedException("Not implemented all missing info yet");
        }

        #region abstract methods

        protected abstract void SpecificInit(TNew newPrimitive, TSnapped snapped);
        protected abstract Term GetTopRadius(TSnapped snapped);
        protected abstract Term GetBottomRadius(TSnapped snapped);

        #endregion

        private Tuple<Term, Term[]> TwoSilhouettesNoFeatures(TSnapped snappedPrimitive, ISet<FeatureCurve> annotated)
        {
            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[0].Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[1].Points);
            var axis2d = Get2DVector(snappedPrimitive.AxisResult);

            var sil0Top = GetForwardPoint(sil0, axis2d);
            var sil0Bot = GetForwardPoint(sil0, -axis2d);
            var sil1Top = GetForwardPoint(sil1, axis2d);
            var sil1Bot = GetForwardPoint(sil1, -axis2d);

            var topFit = ProjectionFit(snappedPrimitive.TopFeatureCurve, new Point[] { sil0Top, sil1Top });
            var botFit = ProjectionFit(snappedPrimitive.BottomFeatureCurve, new Point[] { sil0Bot, sil1Bot });

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
            var sil0 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[0].Points);
            var sil1 = SegmentApproximator.ApproximateSegment(snappedPrimitive.Silhouettes[1].Points);

            var snappedFeatureCurve = snappedPrimitive.TopCurve == null ? snappedPrimitive.BottomFeatureCurve : snappedPrimitive.TopFeatureCurve;
            var unsnappedFeatureCurve = snappedPrimitive.TopCurve == null ? snappedPrimitive.TopFeatureCurve : snappedPrimitive.BottomFeatureCurve;

            var sil0Far = GetFarPoint(sil0, snappedFeatureCurve.SnappedTo);
            var sil1Far = GetFarPoint(sil1, snappedFeatureCurve.SnappedTo);

            var featureProj = ProjectionFit(snappedFeatureCurve);
            var farProj = ProjectionFit(unsnappedFeatureCurve, new Point[] { sil0Far, sil1Far });

            if (annotated.Contains(unsnappedFeatureCurve))
                farProj = Enumerable.Empty<Term>();

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
                from item in snappedPrimitive.FeatureCurves.Cast<CircleFeatureCurve>()
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


        protected IEnumerable<Term> ProjectionFit(CircleFeatureCurve item)
        {
            const int SAMPLE_SIZE = 10;
            var sample = CurveSampler.UniformSample(item.SnappedTo, SAMPLE_SIZE);
            return ProjectionFit(item, sample);
        }

        /// <summary>
        /// Generates a set of terms, one for each given 2D point, that measure the fitness of each point to being
        /// a 2D projection of the given set.
        /// </summary>
        /// <param name="pointsSet">A representation for the 3D points set</param>
        /// <param name="sample">The set of 2D points</param>
        /// <returns>The set of terms, one for each point in <paramref name="sample"/> that measures the fitness of each such point
        /// to the set in <paramref name="pointsSet"/>.</returns>
        protected IEnumerable<Term> ProjectionFit(CircleFeatureCurve pointsSet, Point[] sample)
        {
            var terms =
                from point in sample
                select ProjectionFit(pointsSet, point);

            return terms;
        }

        /// <summary>
        /// Generates a term that gets smaller as the given 2D point fits a 3D points set projection.
        /// </summary>
        /// <param name="pointsSet">A representation for the 3D points set</param>
        /// <param name="point">The 2D point</param>
        /// <returns>The term that measures fitness of <paramref name="point"/> being on the 2D projection of the set specified by <paramref name="pointsSet"/></returns>
        protected Term ProjectionFit(CircleFeatureCurve pointsSet, Point point)
        {
            // here we explicitly assume that the view vector is (0, 0, 1) or (0, 0, -1)
            var x_ = point.X;
            var y_ = point.Y;

            var cx = pointsSet.Center.X;
            var cy = pointsSet.Center.Y;
            var cz = pointsSet.Center.Z;

            var nx = pointsSet.Normal.X;
            var ny = pointsSet.Normal.Y;
            var nz = pointsSet.Normal.Z;

            var r = pointsSet.Radius;

            var dx = cx - x_;
            var dy = cy + y_;

            var lhs = TermBuilder.Sum(
                TermBuilder.Power(dx * nz, 2),
                TermBuilder.Power(dy * nz, 2),
                TermBuilder.Power(dx * nx + dy * ny, 2));
            var rhs = TermBuilder.Power(r * nz, 2);

            return TermBuilder.Power(lhs - rhs, 2);
        }

    }
}
