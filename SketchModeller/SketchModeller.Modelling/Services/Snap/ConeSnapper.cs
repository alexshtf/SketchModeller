using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using System.Diagnostics;
using Utils;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.Snap
{
    class ConeSnapper : BasePrimitivesSnapper<NewCone, SnappedCone>
    {
        protected override SnappedCone Create(PointsSequence[] selectedCurves, NewCone newPrimitive)
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

        protected override Tuple<Term, Term[]> Reconstruct(SnappedCone snappedPrimitive)
        {
            var topCurve = snappedPrimitive.TopCurve;
            var botCurve = snappedPrimitive.BottomCurve;

            // simple case - we use the two curves to reconstruct the cylinder
            if (topCurve != null && botCurve != null)
                return FullInfoObjective(snappedPrimitive);

            throw new NotImplementedException("Not implemented all missing info yet");
        }

        private SnappedCone InitNewSnapped(NewCone selectedCone)
        {
            var snappedCone = new SnappedCone();
            snappedCone.Axis = SnapperHelper.GenerateVarVector();
            snappedCone.BottomCenter = SnapperHelper.GenerateVarVector();
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
}
