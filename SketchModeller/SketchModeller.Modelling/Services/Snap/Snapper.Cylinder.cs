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

namespace SketchModeller.Modelling.Services.Snap
{
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
            snappedCylinder.AxisNormal = GenerateVarVector();
            snappedCylinder.BottomCenter = GenerateVarVector();
            snappedCylinder.Length = new Variable();
            snappedCylinder.Radius = new Variable();

            snappedCylinder.AxisResult = selectedCylinder.Axis.Normalized();
            snappedCylinder.AxisNormalResult = MathUtils3D.NormalVector(snappedCylinder.AxisResult);
            snappedCylinder.BottomCenterResult = selectedCylinder.Bottom;
            snappedCylinder.RadiusResult = selectedCylinder.Radius;
            snappedCylinder.LengthResult = selectedCylinder.Length;

            var secondNormal = TVec.CrossProduct(snappedCylinder.Axis, snappedCylinder.AxisNormal);
            var topCirclePoints =
                CirclePoints(
                    center: snappedCylinder.GetTopCenter(),
                    u: snappedCylinder.AxisNormal,
                    v: secondNormal,
                    radius: snappedCylinder.Radius,
                    count: CIRCLE_POINTS_COUNT);
            var bottomCirclePoints =
                CirclePoints(
                    center: snappedCylinder.BottomCenter,
                    u: snappedCylinder.AxisNormal,
                    v: secondNormal,
                    radius: snappedCylinder.Radius,
                    count: CIRCLE_POINTS_COUNT);

            var allSequences = selectedPolylines.Cast<PointsSequence>().Concat(selectedPolygons).ToArray();
            snappedCylinder.SnappedTo = allSequences;

            // create some parametric representation of the selected cylinder
            var circleCategory = new CurveCategory("Circle");
            var silhouetteCategory = new CurveCategory("Silhouette");

            // compute initial categories
            var seqDictionary = allSequences.ToDictionary(
                seq => seq,
                seq =>
                {
                    if (seq.CurveCategory == CurveCategories.Feature)
                        return circleCategory;
                    else if (seq.CurveCategory == CurveCategories.Silhouette)
                        return silhouetteCategory;
                    else
                        return null;
                });

            var categories = Categorize(seqDictionary, circleCategory, silhouetteCategory);
            if (categories != null)
            {
                var circles = categories.Where(x => x.Value == circleCategory).Select(x => x.Key).ToArray();
                PointsSequence topCircle;
                PointsSequence bottomCircle;
                SelectCircles(selectedCylinder, circles, out topCircle, out bottomCircle);

                snappedCylinder.SnappedPointsSets = new SnappedPointsSet[]
                {
                    new SnappedPointsSet(bottomCirclePoints, bottomCircle),
                    new SnappedPointsSet(topCirclePoints, topCircle),
                };
            }
            else
                logger.Log("Unable to determine curves categories", Category.Info, Priority.None);
            return snappedCylinder;
        }

        private void UpdateDataTerm(SnappedCylinder snappedCylinder)
        {
            var orthogonality =
                TermBuilder.Power(snappedCylinder.Axis * snappedCylinder.AxisNormal, 2) *
                TermBuilder.Power(snappedCylinder.Axis.NormSquared * snappedCylinder.AxisNormal.NormSquared, -1);

            // TODO: Perform snapping to silhouette in the future. Meanwhile we snap only feature lines
            var orthonormality =
                orthogonality +
                TermBuilder.Power(snappedCylinder.Axis.NormSquared - 1, 2) +
                TermBuilder.Power(snappedCylinder.AxisNormal.NormSquared - 1, 2);

            var variables =
                new VariableVectorsWriter()
                .Write(snappedCylinder.Axis)
                .Write(snappedCylinder.AxisNormal)
                .Write(snappedCylinder.BottomCenter)
                .Write(snappedCylinder.Radius)
                .Write(snappedCylinder.Length)
                .ToArray();

            var values =
                new VectorsWriter()
                .Write(snappedCylinder.AxisResult)
                .Write(snappedCylinder.AxisNormalResult)
                .Write(snappedCylinder.BottomCenterResult)
                .Write(snappedCylinder.RadiusResult)
                .Write(snappedCylinder.LengthResult)
                .ToArray();

            var projConstraints = new Term[snappedCylinder.SnappedPointsSets.Length];
            for (int i = 0; i < projConstraints.Length; ++i)
            {
                var snappedPointsSet = snappedCylinder.SnappedPointsSets[i];
                var terms = snappedPointsSet.PointTerms;
                var curve = snappedPointsSet.SnappedTo;
                var constraint = ProjectionConstraint(snappedCylinder, terms, curve, variables, values);
                projConstraints[i] = constraint;
            }

            var finalTerm = TermBuilder.Sum(projConstraints.Append(orthonormality));
            snappedCylinder.DataTerm = finalTerm;
        }

        private Term ProjectionConstraint(
            SnappedCylinder cylinder, 
            ReadOnlyCollection<TVec> terms, 
            PointsSequence curve,
            Variable[] variables,
            double[] values)
        {
            var sample = CurveSampler.UniformSample(curve, terms.Count);
            var sampleTerms = sample.Select(pnt => new TVec(pnt.X, -pnt.Y)).ToArray();
            var projTerms = terms.Select(vec => new TVec(vec[0], vec[1])).ToArray();

            var bestTerm = GeometricTests.DiffSquared(sampleTerms, projTerms);
            var bestValue = Evaluator.Evaluate(bestTerm, variables, values);
            int bestTermIndex = 0;
            for (int i = 0; i < terms.Count - 1; ++i)
            {
                ArrayUtils.RotateRight(sampleTerms);
                var term = GeometricTests.DiffSquared(sampleTerms, projTerms);
                var value = Evaluator.Evaluate(term, variables, values);
                if (value < bestValue)
                {
                    bestTerm = term;
                    bestTermIndex = i + 1;
                }
            }

            var normalizationFactor = 1 / (double)terms.Count;
            return normalizationFactor * bestTerm;
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

        private static TVec[] CirclePoints(TVec center, TVec u, TVec v, Term radius, int count)
        {
            var circlePoints = new TVec[count];
            for (int i = 0; i < count; ++i)
            {
                var fraction = i / (double)count;
                var angle = 2 * Math.PI * fraction;
                circlePoints[i] = center + radius * (u * Math.Cos(angle) + v * Math.Sin(angle));
            }
            return circlePoints;
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
}
