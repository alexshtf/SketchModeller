using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Logging;
using SketchModeller.Infrastructure.Data;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using AutoDiff;
using SketchModeller.Utilities;
using Utils;

using WpfVector3D = System.Windows.Media.Media3D.Vector3D;
using WpfPoint3D = System.Windows.Media.Media3D.Point3D;
using WpfPoint = System.Windows.Point;
using WpfVector = System.Windows.Vector;

namespace SketchModeller.Modelling.Services.Snap
{
    public class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;

        public Snapper(SessionData sessionData, ILoggerFacade logger, IUnityContainer container)
        {
            this.sessionData = sessionData;
            this.logger = logger;
            this.container = container;
        }

        public void Snap()
        {
            var selectedPolylines =
                (from polyline in sessionData.SketchData.Polylines
                 where polyline.IsSelected == true
                 select polyline
                ).ToArray();

            var selectedPolygons =
                (from polygon in sessionData.SketchData.Polygons
                 where polygon.IsSelected == true
                 select polygon
                ).ToArray();

            var selectedCylinder =
                (from cylinder in sessionData.NewPrimitives.OfType<NewCylinder>()
                 where cylinder.IsSelected == true
                 select cylinder
                ).FirstOrDefault();

            // TODO: Find selected primitives of other kinds

            if (selectedCylinder != null)
            {
                SnapCylinder(selectedPolylines, selectedPolygons, selectedCylinder.ToWpf());
                sessionData.NewPrimitives.Remove(selectedCylinder);
            }
        }

        private void SnapCylinder(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCylinderWpf selectedCylinder)
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
                return;
            }

            var circles = categories.Where(x => x.Value == circleCategory).Select(x => x.Key).ToArray();
            PointsSequence topCircle;
            PointsSequence bottomCircle;
            SelectCircles(selectedCylinder, circles, out topCircle, out bottomCircle);

            // TODO: Perform snapping to silhouette in the future. Meanwhile we snap only feature lines
            const int CIRCLE_POINTS_COUNT = 50;
            var axisNorm = selectedCylinder.Axis.Normalized();
            var axis = new TVec(axisNorm.X, axisNorm.Y, axisNorm.Z);

            var t = new Variable();

            // bottom circle parameters
            var c1 = GenerateVarVector();
            var u1 = GenerateVarVector();
            var v1 = TVec.CrossProduct(u1, axis);

            // top circle parameters
            var c2 = c1 + t * axis;
            var u2 = u1;
            var v2 = v1;

            var ptsBottom = CirclePoints(c1, u1, v1, 0.5 * selectedCylinder.Diameter, CIRCLE_POINTS_COUNT);
            var ptsTop = CirclePoints(c2, u2, v2, 0.5 * selectedCylinder.Diameter, CIRCLE_POINTS_COUNT);

            var orthogonality = TermBuilder.Power(u1 * axis, 2) + TermBuilder.Power(u2 * axis, 2);
            var proj1 = ProjectionConstraint(ptsBottom, bottomCircle);
            var proj2 = ProjectionConstraint(ptsTop, topCircle);
            var tSize = TermBuilder.Power(t - selectedCylinder.Length, 2);
            var unitLen = TermBuilder.Power(u1.NormSquared - 1, 2);

            var finalTerm =
                TermBuilder.Sum(
                1 * orthogonality,
                100 * proj1,
                100 * proj2,
                1 * tSize,
                1 * unitLen,
                0);

            var snappedCylinder = OptimizeCylinder(finalTerm, selectedCylinder, CIRCLE_POINTS_COUNT, c1, u1, t);
            snappedCylinder.SnappedTo = allSequences;

            sessionData.SnappedPrimitives.Add(snappedCylinder);
        }

        private void SelectCircles(NewCylinderWpf selectedCylinder, PointsSequence[] circles, out PointsSequence topCircle, out PointsSequence bottomCircle)
        {
            Contract.Requires(circles.Length >= 2);
            Contract.Ensures(Contract.ValueAtReturn(out topCircle) != Contract.ValueAtReturn(out bottomCircle));

            var top = new WpfPoint(selectedCylinder.Top.X, -selectedCylinder.Top.Y);
            var bottom = new WpfPoint(selectedCylinder.Bottom.X, -selectedCylinder.Bottom.Y);

            Func<PointsSequence, WpfPoint, double> distance = (curve, pnt) =>
                {
                    var sample = CurveSampler.UniformSample(curve, 50);
                    var result = pnt.ProjectionOnCurve(sample.ToWpfPoints()).Item2;
                    return result;
                };

            topCircle = circles.Minimizer(circle => distance(circle, top));
            bottomCircle = circles.Minimizer(circle => distance(circle, bottom));
        }

        private SnappedCylinder OptimizeCylinder(Term finalTerm, NewCylinderWpf selectedCylinder, int count, TVec vc1, TVec vu1, Variable vt)
        {
            var vars = GetVars(new TVec[] { vc1, vu1 }).Append(vt).ToArray();
            double[] guess = CreateStartGuess(selectedCylinder);
            double[] optimals = Optimizer.Minimize(finalTerm, vars, guess);

            var axis = selectedCylinder.Axis.Normalized();

            // bottom circle params
            var c1 = new WpfPoint3D(optimals[0], optimals[1], optimals[2]);
            var u1 = new WpfVector3D(optimals[3], optimals[4], optimals[5]);
            var v1 = WpfVector3D.CrossProduct(u1, axis);
            var t = optimals[6];

            // top circle params
            var c2 = c1 + t * axis;
            var u2 = u1;
            var v2 = v1;

            var botCircle = CirclePoints(c1, u1, v1, selectedCylinder.Diameter / 2, count);
            var topCircle = CirclePoints(c2, u2, v2, selectedCylinder.Diameter / 2, count);

            return new SnappedCylinder 
            { 
                TopCircle = topCircle.ToDataPoints().ToArray(), 
                BottomCircle = botCircle.ToDataPoints().ToArray(),
            };
        }

        private double[] CreateStartGuess(NewCylinderWpf selectedCylinder)
        {
            var axisShifted = selectedCylinder.Axis;
            axisShifted.X += 1;
            axisShifted.Y += 2;
            axisShifted.Z += 3;

            var c = selectedCylinder.Bottom;
            var u = MathUtils3D.MostSimilarPerpendicular(axisShifted, selectedCylinder.Axis).Normalized();
            var t = selectedCylinder.Length;

            return new double[] { c.X, c.Y, c.Z, u.X, u.Y, u.Z, t };
        }

        private Term ProjectionConstraint(TVec center, TVec u, TVec v, Term radius, PointsSequence pointsSequence, int count)
        {
            var circlePoints = CirclePoints(center, u, v, radius, count);
            return ProjectionConstraint(circlePoints, pointsSequence);
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

        private static WpfPoint3D[] CirclePoints(WpfPoint3D center, WpfVector3D u, WpfVector3D v, double radius, int count)
        {
            var circlePoints = new WpfPoint3D[count];
            for (int i = 0; i < count; ++i)
            {
                var fraction = i / (double)count;
                var angle = 2 * Math.PI * fraction;
                circlePoints[i] = center + radius * (u * Math.Cos(angle) + v * Math.Sin(angle));
            }
            return circlePoints;
        }

        private Term ProjectionConstraint(TVec[] curveVars, PointsSequence projCurve)
        {
            var samples = CurveSampler.UniformSample(projCurve, curveVars.Length);
            var sampleTerms =
                from sample in samples
                select new TVec(sample.X, -sample.Y);

            var curveProj =
                from pnt in curveVars
                select new TVec(pnt[0], pnt[1]); // take only X and Y as projection operator

            return GeometricTests.DiffSquared(sampleTerms.ToArray(), curveProj.ToArray());
        }

        private Variable[] GetVars(IEnumerable<TVec> vectors)
        {
            return 
                vectors
                .SelectMany(vec => vec.GetTerms())
                .Cast<Variable>()
                .ToArray();
        }

        private TVec[] GenerateVarVectors(int count)
        {
            var result = new TVec[count];
            for (int i = 0; i < count; ++i)
                result[i] = GenerateVarVector();

            return result;
        }

        private static TVec GenerateVarVector()
        {
            return new TVec(new Variable(), new Variable(), new Variable());
        }

        private Dictionary<PointsSequence, CurveCategory> Categorize(IEnumerable<PointsSequence> sequences, params CurveCategory[] categories)
        {
            // create viewmode/view for the categorizer window
            var categorizerVM = container.Resolve<CategorizerViewModel>();
            var categorizerView = container.Resolve<CategorizerView>(new DependencyOverride<CategorizerViewModel>(categorizerVM));

            // display the categorizer window with the correct data
            categorizerVM.Setup(sequences, categories);
            categorizerView.ShowDialog();

            // extract data from the categorizer
            if (categorizerVM.IsFinished)
                return categorizerVM.Result;
            else
                return null;
        }

        public class CurveCategory
        {
            public CurveCategory(string name)
            {
                Name = name;
            }

            public string Name { get; private set; }
        }
    }
}
