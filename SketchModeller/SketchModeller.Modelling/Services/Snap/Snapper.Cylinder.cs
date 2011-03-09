using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using AutoDiff;
using Utils;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;
using System.Windows;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Prism.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class NewSnapper
    {
        private SnappedCylinder CreateSnappedCylinder(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCylinder selectedCylinder)
        {
            var snappedCylinder = InitNewSnapped(selectedCylinder);

            var allSequences = selectedPolylines.Cast<PointsSequence>().Concat(selectedPolygons).ToArray();
            snappedCylinder.SnappedTo = allSequences;

            var features = allSequences.Where(x => x.CurveCategory == CurveCategories.Feature).ToArray();
            var silhouettes = allSequences.Except(features).ToArray();

            var topPts = features.Where(x => CylinderHelper.IsTop(x, snappedCylinder)).ToArray();
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

        private Tuple<Term, Term[]> Reconstruct(SnappedCylinder cylinder)
        {
            var topCurve = cylinder.TopCurve;
            var botCurve = cylinder.BottomCurve;

            // simple case - we use the two curves to reconstruct the cylinder
            if (topCurve != null && botCurve != null)
                return FullInfoObjective(cylinder);

            throw new NotImplementedException("Not implemented all missing info yet");
        }

        private Tuple<Term, Term[]> FullInfoObjective(SnappedCylinder cylinder)
        {
            var terms =
                from item in cylinder.SnappedPointsSets
                from term in ProjectionConstraints(item)
                select term;

            var objective = TermUtils.SafeAvg(terms);
            var constraints = new Term[] { cylinder.Axis.NormSquared - 1 };

            return Tuple.Create(objective, constraints);
        }

        private static SnappedCylinder InitNewSnapped(NewCylinder selectedCylinder)
        {
            var snappedCylinder = new SnappedCylinder();
            snappedCylinder.Axis = GenerateVarVector();
            snappedCylinder.BottomCenter = GenerateVarVector();
            snappedCylinder.Length = new Variable();
            snappedCylinder.Radius = new Variable();

            snappedCylinder.AxisResult = selectedCylinder.Axis.Normalized();
            snappedCylinder.BottomCenterResult = selectedCylinder.Bottom;
            snappedCylinder.RadiusResult = selectedCylinder.Radius;
            snappedCylinder.LengthResult = selectedCylinder.Length;
            return snappedCylinder;
        }

        private static class CylinderHelper
        {
            /// <summary>
            /// Checks weather a points sequence is the top-part of a cylinder, by comparing the center of the ellipse that best fits the points
            /// sequence to the actual top center and bottom centers.
            /// </summary>
            /// <param name="points">The points sequence</param>
            /// <param name="cylinder">The cylinder data</param>
            /// <returns><c>true</c> if the best-fit-ellipse's center is closer to the top cylinder point than to the bottom.</returns>
            public static bool IsTop(PointsSequence points, dynamic cylinder)
            {
                var top = new Point(cylinder.TopCenterResult.X, -cylinder.TopCenterResult.Y);
                var bottom = new Point(cylinder.BottomCenterResult.X, -cylinder.BottomCenterResult.Y);

                var samples = CurveSampler.UniformSample(points, 50);
                var ellipse = EllipseFitter.Fit(samples);

                // the points sequence is "top" if it is closer to the top center then it is to the bottom center.
                if ((top - ellipse.Center).Length < (bottom - ellipse.Center).Length)
                    return true;
                else
                    return false;
            }
        }
    }

    #region old code
    /*
    public partial class Snapper
    {
        const int CIRCLE_POINTS_COUNT = 20;

        private SnappedCylinder SnapCylinder(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCylinder selectedCylinder)
        {
            var snappedCylinder = Init(selectedPolylines, selectedPolygons, selectedCylinder);
            if (snappedCylinder != null)
                UpdateDataTerm(snappedCylinder);
            return snappedCylinder;
        }

        private SnappedCylinder Init(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCylinder selectedCylinder)
        {
            var snappedCylinder = new SnappedCylinder();
            snappedCylinder.Axis = GenerateVarVector();
            snappedCylinder.BottomCenter = GenerateVarVector();
            snappedCylinder.Length = new Variable();
            snappedCylinder.Radius = new Variable();

            snappedCylinder.AxisResult = selectedCylinder.Axis.Normalized();
            snappedCylinder.BottomCenterResult = selectedCylinder.Bottom;
            snappedCylinder.RadiusResult = selectedCylinder.Radius;
            snappedCylinder.LengthResult = selectedCylinder.Length;

            var allSequences = selectedPolylines.Cast<PointsSequence>().Concat(selectedPolygons).ToArray();
            snappedCylinder.SnappedTo = allSequences;

            var circles = allSequences.Where(x => x.CurveCategory == CurveCategories.Feature).ToArray();
            if (circles.Length == 2)
            {
                PointsSequence topCircle;
                PointsSequence bottomCircle;
                SelectCircles(selectedCylinder, circles, out topCircle, out bottomCircle);

                snappedCylinder.SnappedPointsSets = new SnappedPointsSet[]
                {
                    new SnappedPointsSet(snappedCylinder.BottomCenter, snappedCylinder.Axis, snappedCylinder.Radius, bottomCircle),
                    new SnappedPointsSet(snappedCylinder.GetTopCenter(), snappedCylinder.Axis, snappedCylinder.Radius, topCircle),
                };
            }
            else
                logger.Log("Unable to determine curves categories", Category.Info, Priority.None);
            return snappedCylinder;
        }

        private void UpdateDataTerm(SnappedCylinder snappedCylinder)
        {
            var terms =
                from item in snappedCylinder.SnappedPointsSets
                from term in ProjectionConstraint(item)
                select term;
            
            var normalization = 10000 * TermBuilder.Power(snappedCylinder.Axis.NormSquared - 1, 2);
            terms = terms.Append(normalization);

            snappedCylinder.DataTerm = TermUtils.SafeSum(terms);
        }

        private IEnumerable<Term> ProjectionConstraint(SnappedPointsSet item)
        {
            const int SAMPLE_SIZE = 10;
            var sample = CurveSampler.UniformSample(item.SnappedTo, SAMPLE_SIZE);
            var terms =
                from point in sample
                from term in ProjectionConstraint(item, point)
                select term;

            return terms;
        }

        private IEnumerable<Term> ProjectionConstraint(SnappedPointsSet item, Point point)
        {
            // here we explicitly assume that the view vector is (0, 0, 1) or (0, 0, -1)
            var x_ = point.X;
            var y_ = point.Y;

            var cx = item.Center.X;
            var cy = item.Center.Y;
            var cz = item.Center.Z;
            
            var nx = item.Axis.X;
            var ny = item.Axis.Y;
            var nz = item.Axis.Z;
            
            var r = item.Radius;

            var dx = cx - x_;
            var dy = cy + y_;

            var lhs = TermBuilder.Sum(
                TermBuilder.Power(dx * nz, 2),
                TermBuilder.Power(dy * nz, 2),
                TermBuilder.Power(dx * nx + dy * ny, 2));
            var rhs = TermBuilder.Power(r * nz, 2);

            yield return TermBuilder.Power(lhs - rhs, 2);
        }

        private void SelectCircles(NewCylinder selectedCylinder, PointsSequence[] circles, out PointsSequence topCircle, out PointsSequence bottomCircle)
        {
            Contract.Requires(circles.Length >= 2);
            Contract.Ensures(Contract.ValueAtReturn(out topCircle) != Contract.ValueAtReturn(out bottomCircle));

            var top = new Point(selectedCylinder.Top.X, -selectedCylinder.Top.Y);
            var bottom = new Point(selectedCylinder.Bottom.X, -selectedCylinder.Bottom.Y);

            Func<PointsSequence, Point, double> distance = (curve, pnt) =>
            {
                var sample = CurveSampler.UniformSample(curve, 50);
                var result = pnt.ProjectionOnCurve(sample).Item2;
                return result;
            };

            topCircle = circles.Minimizer(circle => distance(circle, top));
            bottomCircle = circles.Minimizer(circle => distance(circle, bottom));
        }

        private static Point3D[] CirclePoints(Point3D center, Vector3D u, Vector3D v, double radius, int count)
        {
            var circlePoints = new Point3D[count];
            for (int i = 0; i < count; ++i)
            {
                var fraction = i / (double)count;
                var angle = 2 * Math.PI * fraction;
                circlePoints[i] = center + radius * (u * Math.Cos(angle) + v * Math.Sin(angle));
            }
            return circlePoints;
        }
    }
    */
    #endregion
}
