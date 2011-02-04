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
            snappedCylinder.BottomCenter = GenerateVarVector();
            snappedCylinder.Length = new Variable();
            snappedCylinder.Radius = new Variable();

            snappedCylinder.AxisResult = selectedCylinder.Axis.Normalized();
            snappedCylinder.BottomCenterResult = selectedCylinder.Bottom;
            snappedCylinder.RadiusResult = selectedCylinder.Radius;
            snappedCylinder.LengthResult = selectedCylinder.Length;

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
            const int SAMPLE_SIZE = 3;
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
}
