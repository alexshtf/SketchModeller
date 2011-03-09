using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using Utils;
using Microsoft.Practices.Prism.Logging;
using System.Diagnostics.Contracts;
using System.Windows;
using SketchModeller.Utilities;
using System.Diagnostics;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class NewSnapper
    {
        private SnappedCone CreateSnappedCone(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCone selectedCone)
        {
            var snappedCone = InitNewSnapped(selectedCone);

            var allSequences = selectedPolylines.Cast<PointsSequence>().Concat(selectedPolygons).ToArray();
            snappedCone.SnappedTo = allSequences;

            var features = allSequences.Where(x => x.CurveCategory == CurveCategories.Feature).ToArray();
            var silhouettes = allSequences.Except(features).ToArray();

            var topPts = features.Where(x => CylinderHelper.IsTop(x, snappedCone)).ToArray();
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
                        snappedCone.TopRadius,
                        topPts[0]));
            if (botPts.Length > 0)
                pointsSets.Add(
                    new SnappedPointsSet(
                        snappedCone.BottomCenter,
                        snappedCone.Axis,
                        snappedCone.BottomRadius,
                        botPts[0]));
            snappedCone.SnappedPointsSets = pointsSets.ToArray();

            return snappedCone;
        }

        private SnappedCone InitNewSnapped(NewCone selectedCone)
        {
            var snappedCone = new SnappedCone();
            snappedCone.Axis = GenerateVarVector();
            snappedCone.BottomCenter = GenerateVarVector();
            snappedCone.Length = new Variable();
            snappedCone.TopRadius = new Variable();
            snappedCone.BottomRadius = new Variable();

            snappedCone.AxisResult = selectedCone.Axis.Normalized();
            snappedCone.BottomCenterResult = selectedCone.Bottom;
            snappedCone.BottomRadiusResult = selectedCone.BottomRadius;
            snappedCone.TopRadiusResult = selectedCone.TopRadius;
            snappedCone.LengthResult = selectedCone.Length;

            return snappedCone;
        }

        private Tuple<Term, Term[]> Reconstruct(SnappedCone cone)
        {
            var topCurve = cone.TopCurve;
            var botCurve = cone.BottomCurve;

            // simple case - we use the two curves to reconstruct the cylinder
            if (topCurve != null && botCurve != null)
                return FullInfoObjective(cone);

            throw new NotImplementedException("Not implemented all missing info yet");
        }

        private Tuple<Term, Term[]> FullInfoObjective(SnappedCone cone)
        {
            var terms =
                from item in cone.SnappedPointsSets
                from term in ProjectionConstraints(item)
                select term;
            var objective = TermUtils.SafeAvg(terms);
            var constraints = new Term[] { cone.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }
    }

    #region old code
    /*
    public partial class Snapper
    {
        public SnappedCone SnapCone(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCone selectedCone)
        {
            var snappedCone = Init(selectedPolylines, selectedPolygons, selectedCone);
            if (snappedCone != null)
                UpdateDataTerm(snappedCone);
            return snappedCone;
        }

        private SnappedCone Init(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCone selectedCone)
        {
            var snappedCone = new SnappedCone();
            snappedCone.Axis = GenerateVarVector();
            snappedCone.BottomCenter = GenerateVarVector();
            snappedCone.Length = new Variable();
            snappedCone.TopRadius = new Variable();
            snappedCone.BottomRadius = new Variable();

            snappedCone.AxisResult = selectedCone.Axis.Normalized();
            snappedCone.BottomCenterResult = selectedCone.Bottom;
            snappedCone.BottomRadiusResult = selectedCone.BottomRadius;
            snappedCone.TopRadiusResult = selectedCone.TopRadius;
            snappedCone.LengthResult = selectedCone.Length;

            var allSequences = selectedPolylines.Cast<PointsSequence>().Concat(selectedPolygons).ToArray();
            snappedCone.SnappedTo = allSequences;

            var circles = allSequences.Where(x => x.CurveCategory == CurveCategories.Feature).ToArray();
            if (circles.Length == 2)
            {
                PointsSequence topCircle;
                PointsSequence bottomCircle;
                SelectCircles(selectedCone, circles, out topCircle, out bottomCircle);

                snappedCone.SnappedPointsSets = new SnappedPointsSet[]
                {
                    new SnappedPointsSet(snappedCone.BottomCenter, snappedCone.Axis, snappedCone.BottomRadius, bottomCircle),
                    new SnappedPointsSet(snappedCone.GetTopCenter(), snappedCone.Axis, snappedCone.TopRadius, topCircle),
                };
            }
            else
                logger.Log("Unable to determine curves categories", Category.Info, Priority.None);
            return snappedCone;
        }

        private void SelectCircles(NewCone selectedCone, PointsSequence[] circles, out PointsSequence topCircle, out PointsSequence bottomCircle)
        {
            Contract.Requires(circles.Length >= 2);
            Contract.Ensures(Contract.ValueAtReturn(out topCircle) != Contract.ValueAtReturn(out bottomCircle));

            var top = new Point(selectedCone.Top.X, -selectedCone.Top.Y);
            var bottom = new Point(selectedCone.Bottom.X, -selectedCone.Bottom.Y);

            Func<PointsSequence, Point, double> distance = (curve, pnt) =>
            {
                var sample = CurveSampler.UniformSample(curve, 50);
                var result = pnt.ProjectionOnCurve(sample).Item2;
                return result;
            };

            topCircle = circles.Minimizer(circle => distance(circle, top));
            bottomCircle = circles.Minimizer(circle => distance(circle, bottom));
        }

        private void UpdateDataTerm(SnappedCone snappedCone)
        {
            var terms =
                from item in snappedCone.SnappedPointsSets
                from term in ProjectionConstraint(item)
                select term;

            var normalization = 10000 * TermBuilder.Power(snappedCone.Axis.NormSquared - 1, 2);
            terms = terms.Append(normalization);

            snappedCone.DataTerm = TermUtils.SafeSum(terms);
        }
    }
    */
    #endregion

}
