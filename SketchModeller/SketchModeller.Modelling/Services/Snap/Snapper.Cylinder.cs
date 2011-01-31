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

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper
    {
        private SnappedCylinder SnapCylinder(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCylinder selectedCylinder)
        {
            var allSequences = selectedPolylines.Cast<PointsSequence>().Concat(selectedPolygons).ToArray();

            // create some parametric representation of the selected cylinder
            var circleCategory = new CurveCategory("Circle");
            var silhouetteCategory = new CurveCategory("Silhouette");

            var categories = Categorize(allSequences, circleCategory, silhouetteCategory);

            // we could not find categories, so we cancel the snapping process
            if (categories == null)
            {
                logger.Log("Unable to determine curves categories", Category.Info, Priority.None);
                return null;
            }

            var circles = categories.Where(x => x.Value == circleCategory).Select(x => x.Key).ToArray();
            PointsSequence topCircle;
            PointsSequence bottomCircle;
            SelectCircles(selectedCylinder, circles, out topCircle, out bottomCircle);

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

            // TODO: Perform snapping to silhouette in the future. Meanwhile we snap only feature lines

            var orthonormality =
                TermBuilder.Power(snappedCylinder.Axis * snappedCylinder.AxisNormal, 2) +
                TermBuilder.Power(snappedCylinder.Axis.NormSquared - 1, 2) +
                TermBuilder.Power(snappedCylinder.AxisNormal.NormSquared - 1, 2);

            const int CIRCLE_POINTS_COUNT = 20;
            var bottomProjConstraint = 
                CircleProjectionConstraint(
                    bottomCircle, 
                    snappedCylinder, 
                    selectedCylinder.Bottom, 
                    snappedCylinder.BottomCenter, 
                    CIRCLE_POINTS_COUNT);
            var topProjConstraint =
                CircleProjectionConstraint(
                    topCircle,
                    snappedCylinder,
                    selectedCylinder.Top,
                    snappedCylinder.BottomCenter + snappedCylinder.Length * snappedCylinder.Axis,
                    CIRCLE_POINTS_COUNT);

            var topPointTerms = 
                CirclePoints(
                    center:     snappedCylinder.BottomCenter,
                    u:          snappedCylinder.AxisNormal,
                    v:          TVec.CrossProduct(snappedCylinder.Axis, snappedCylinder.AxisNormal),
                    radius:     snappedCylinder.Radius,
                    count:      CIRCLE_POINTS_COUNT);

            var finalTerm =
                orthonormality +
                bottomProjConstraint.Item1 +
                topProjConstraint.Item1;

            snappedCylinder.DataTerm = finalTerm;
            snappedCylinder.SnappedTo = allSequences;
            snappedCylinder.SnappedPointsSets = new SnappedPointsSet[]
            {
                new SnappedPointsSet(bottomProjConstraint.Item2, bottomCircle),
                new SnappedPointsSet(topProjConstraint.Item2, topCircle),
            };

            return snappedCylinder;
        }

        private Tuple<Term, TVec[]> CircleProjectionConstraint(PointsSequence circle2d, SnappedCylinder snappedCylinder, Point3D realCenter, TVec termCenter, int count)
        {
            var projectedCircle =
                GetProjectedCircle(
                    circle2d,
                    realCenter,
                    snappedCylinder.AxisResult,
                    snappedCylinder.AxisNormalResult,
                    snappedCylinder.RadiusResult,
                    count);
            var terms =
                CirclePoints(
                    center: termCenter,
                    u: snappedCylinder.AxisNormal,
                    v: TVec.CrossProduct(snappedCylinder.Axis, snappedCylinder.AxisNormal),
                    radius: snappedCylinder.Radius,
                    count: count);
            var bottomProjConstraint = ProjectionConstraint(terms, projectedCircle);
            return Tuple.Create(bottomProjConstraint, terms);
        }

        private Term ProjectionConstraint(TVec[] terms, Point[] points2d)
        {
            var sampleTerms =
                from sample in points2d
                select new TVec(sample.X, -sample.Y);

            var curveProj =
                from pnt in terms
                select new TVec(pnt[0], pnt[1]); // take only X and Y as projection operator

            var factor = 1 / (double)terms.Length;

            return factor * GeometricTests.DiffSquared(sampleTerms.ToArray(), curveProj.ToArray());
        }

        private static Point[] GetProjectedCircle(PointsSequence projectOn, Point3D center, Vector3D axis, Vector3D normal, double radius, int count)
        {
            var points3d =
                CirclePoints(
                    center: center,
                    u: normal,
                    v: Vector3D.CrossProduct(axis, normal),
                    radius: radius,
                    count: count);
            var projectedPoints =
                (from pnt3d in points3d
                 let pnt2d = new Point(pnt3d.X, -pnt3d.Y)
                 let curve = projectOn is Polygon 
                           ? projectOn.Points.Append(projectOn.Points.First())  // close the curve if it is a polygon
                           : projectOn.Points
                 let proj = pnt2d.ProjectionOnCurve(curve).Item1
                 select proj
                ).ToArray();
            return projectedPoints;
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
                var angle = 2 * Math.PI * i;
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
