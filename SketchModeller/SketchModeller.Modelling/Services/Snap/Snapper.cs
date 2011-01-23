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
                SnapCylinder(selectedPolylines, selectedPolygons, selectedCylinder);
        }

        private void SnapCylinder(Polyline[] selectedPolylines, Polygon[] selectedPolygons, NewCylinder selectedCylinder)
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

            // TODO: Perform snapping to silhouette in the future. Meanwhile we snap only feature lines
            const int CIRCLE_POINTS_COUNT = 50;
            TVec[] topCircleVars = GenerateVarVectors(CIRCLE_POINTS_COUNT);
            TVec[] botCircleVars = GenerateVarVectors(CIRCLE_POINTS_COUNT);

            var topSphericality = GeometricTests.MultiCosphericality3D(topCircleVars);
            var botSphericality = GeometricTests.MultiCosphericality3D(botCircleVars);
            var topPlanarity = GeometricTests.MultiCoplanarity3D(topCircleVars);
            var botPlanarity = GeometricTests.MultiCoplanarity3D(botCircleVars);
            var topProj = ProjectionConstraint(topCircleVars, circles[0]);
            var botProj = ProjectionConstraint(botCircleVars, circles[1]);
            var topBotProximity = GeometricTests.DiffSquared(topCircleVars, botCircleVars);
            var topAxisParallelism = AxisParallelism(topCircleVars, selectedCylinder.Axis);
            var botAxisParallelism = AxisParallelism(botCircleVars, selectedCylinder.Axis);

            const double PLANARITY_WEIGHT = 1.0;
            const double SPHERICALITY_WEIGHT = 1.0;
            const double PROJ_WEIGHT = 10.0;
            const double PROX_WEIGHT = 0.1;
            const double AXIS_PARALELLISM_WEIGHT = 10.0;

            var finalTerm =
                PLANARITY_WEIGHT * (topPlanarity + botPlanarity) +
                SPHERICALITY_WEIGHT * (topSphericality + botSphericality) +
                PROJ_WEIGHT * (topProj + botProj) +
                PROX_WEIGHT * topBotProximity +
                AXIS_PARALELLISM_WEIGHT * (topAxisParallelism + botAxisParallelism);

            var snappedCylinder = OptimizeCylinder(finalTerm, topCircleVars, botCircleVars, selectedCylinder);
            snappedCylinder.SnappedTo = allSequences;

            sessionData.NewPrimitives.Remove(selectedCylinder);
            sessionData.SnappedPrimitives.Add(snappedCylinder);
        }

        private Term AxisParallelism(TVec[] curveVars, Point3D axis)
        {
            var axisTerm = new TVec(axis.X, axis.Y, axis.Z);
            var sumTerms = from triple in curveVars.SeqTripples()
                           let normal = TermUtils.Normal3D(triple.Item1, triple.Item2, triple.Item3)
                           let term = GeometricTests.VectorParallelism3D(normal, axisTerm)
                           select term;
            return TermBuilder.Sum(sumTerms);
        }

        private SnappedPrimitive OptimizeCylinder(Term finalTerm, TVec[] topCircleVars, TVec[] botCircleVars, NewCylinder selectedCylinder)
        {
            var allVars = GetVars(topCircleVars.Concat(botCircleVars));
            var startVector = CreateStartVector(selectedCylinder, topCircleVars.Length, botCircleVars.Length);
            double[] minimizer = Optimizer.Minimize(finalTerm, allVars, startVector);

            var topCircle = new Point3D[topCircleVars.Length];
            var botCircle = new Point3D[botCircleVars.Length];

            for (int i = 0; i < topCircleVars.Length; ++i)
                topCircle[i] = new Point3D
                {
                    X = minimizer[3 * i + 0],
                    Y = minimizer[3 * i + 1],
                    Z = minimizer[3 * i + 2],
                };

            for (int i = 0, j = topCircleVars.Length; i < botCircleVars.Length; ++i, ++j)
                botCircle[i] = new Point3D
                {
                    X = minimizer[3 * j + 0],
                    Y = minimizer[3 * j + 1],
                    Z = minimizer[3 * j + 2],
                };

            return new SnappedCylinder { TopCircle = topCircle, BottomCircle = botCircle };
        }

        private double[] CreateStartVector(NewCylinder selectedCylinder, int topCircleSize, int botCircleSize)
        {
            var axis = selectedCylinder.Axis.ToWpfVector();
            var center = selectedCylinder.Center.ToWpfPoint();
            var diameter = selectedCylinder.Diameter;
            var length = selectedCylinder.Length;

            // find two basis vectors
            var u = MathUtils3D.MostSimilarPerpendicular(axis + new WpfVector3D(1, 2, 3), axis);
            var v = WpfVector3D.CrossProduct(axis, u);
            u.Normalize();
            v.Normalize();

            var topCircle = GenerateCircle(center + 0.5 * length * axis, u, v, diameter, topCircleSize);
            var botCircle = GenerateCircle(center - 0.5 * length * axis, u, v, diameter, botCircleSize);

            var result = new double[3 * (topCircleSize + botCircleSize)];
            for (int i = 0; i < topCircleSize; ++i)
            {
                result[3 * i + 0] = topCircle[i].X;
                result[3 * i + 1] = topCircle[i].Y;
                result[3 * i + 2] = topCircle[i].Z;
            }
            for (int i = 0; i < botCircleSize; ++i)
            {
                result[3 * topCircleSize + 3 * i + 0] = botCircle[i].X;
                result[3 * topCircleSize + 3 * i + 1] = botCircle[i].Y;
                result[3 * topCircleSize + 3 * i + 2] = botCircle[i].Z;
            }

            return result;
        }

        private static WpfPoint3D[] GenerateCircle(WpfPoint3D center, WpfVector3D u, WpfVector3D v, double diameter, int topCircleSize)
        {
            var topCircle = new WpfPoint3D[topCircleSize];
            for (int i = 0; i < topCircleSize; ++i)
            {
                var fraction = i / (double)topCircleSize;
                var angle = 2 * fraction * Math.PI;
                topCircle[i] = center + 0.5 * diameter * (Math.Cos(angle) * u + Math.Sin(angle) * v);
            }
            return topCircle;
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
                result[i] = new TVec(new Variable(), new Variable(), new Variable());

            return result;
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
