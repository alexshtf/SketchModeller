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

namespace SketchModeller.Modelling.Services.Snap
{
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
}
