﻿using System;
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
using System.Windows.Media.Media3D;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using System.Collections.ObjectModel;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;
        private readonly Random random;

        public Snapper(SessionData sessionData, ILoggerFacade logger, IEventAggregator eventAggregator, IUnityContainer container)
        {
            this.sessionData = sessionData;
            this.logger = logger;
            this.container = container;
            this.eventAggregator = eventAggregator;
            this.random = new Random(100);
        }

        public void Snap()
        {
            var selectedPolylines = sessionData.SelectedSketchObjects.OfType<Polyline>().ToArray();
            var selectedPolygons = sessionData.SelectedSketchObjects.OfType<Polygon>().ToArray();
            var selectedCylinder = sessionData.SelectedNewPrimitives.OfType<NewCylinder>().FirstOrDefault();

            // TODO: Find selected primitives of other kinds

            if (selectedCylinder != null)
            {
                var snappedCylinder = SnapCylinder(selectedPolylines, selectedPolygons, selectedCylinder);
                sessionData.SnappedPrimitives.Add(snappedCylinder);
                sessionData.NewPrimitives.Remove(selectedCylinder);
            }
            OptimizeAll();

            eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
        }

        private void OptimizeAll()
        {
            var variablesWriter = new VariableVectorsWriter();
            var startVectorWriter = new VectorsWriter();

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                variablesWriter
                    .Write(snappedCylinder.Axis)
                    .Write(snappedCylinder.BottomCenter)
                    .Write(snappedCylinder.Length)
                    .Write(snappedCylinder.Radius);
                startVectorWriter
                    .Write(snappedCylinder.AxisResult)
                    .Write(snappedCylinder.BottomCenterResult)
                    .Write(snappedCylinder.LengthResult)
                    .Write(snappedCylinder.RadiusResult);
            }

            var dataTerms = sessionData.SnappedPrimitives.Select(p => p.DataTerm);
            var annotationTerms = sessionData.Annotations.Select(x => GetAnnotationTerm(x));
            var dataObjective = TermUtils.SafeSum(dataTerms);
            var annotationsObjective = TermUtils.SafeSum(annotationTerms);
            var totalObjective = dataObjective + 0.001 * annotationsObjective;

            var variables = variablesWriter.ToArray();
            var startVector = startVectorWriter.ToArray();

            var minimizer = startVector;
            var dataLM = Optimizer.GetLMFuncs(dataObjective);
            var totalLM = Optimizer.GetLMFuncs(totalObjective);
            for (int i = 0; i < 40; ++i)
            {
                minimizer = Optimizer.MinimizeLM(dataLM, variables, minimizer);
                minimizer = Optimizer.MinimizeLM(totalLM, variables, minimizer);
            }

            var resultReader = new VectorsReader(minimizer);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                snappedCylinder.AxisResult = resultReader.ReadVector3D();
                snappedCylinder.BottomCenterResult = resultReader.ReadPoint3D();
                snappedCylinder.LengthResult = resultReader.ReadValue();
                snappedCylinder.RadiusResult = resultReader.ReadValue();
            }

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                var normal = MathUtils3D.NormalVector(snappedCylinder.AxisResult);
                var secondNormal = Vector3D.CrossProduct(normal, snappedCylinder.AxisResult);
                snappedCylinder.BottomCircle = CirclePoints(
                    snappedCylinder.BottomCenterResult,
                    normal,
                    secondNormal,
                    snappedCylinder.RadiusResult,
                    50);

                snappedCylinder.TopCircle = CirclePoints(
                    snappedCylinder.TopCenterResult,
                    normal,
                    secondNormal,
                    snappedCylinder.RadiusResult,
                    50);
            }
        }

        private Term GetAnnotationTerm(Annotation x)
        {
            Term term = 0;
            x.MatchClass<Parallelism>(parallelism => term = GetConcreteAnnotationTerm(parallelism));
            x.MatchClass<Coplanarity>(coplanarity => term = GetConcreteAnnotationTerm(coplanarity));
            return term;
        }

        private Term GetConcreteAnnotationTerm(Coplanarity coplanarity)
        {
            var pointsSetsQuery =
                from curve in coplanarity.Elements
                from snappedPrimitive in sessionData.SnappedPrimitives
                from pointsSet in snappedPrimitive.SnappedPointsSets
                where pointsSet.SnappedTo == curve
                select pointsSet;

            var pointsSets = pointsSetsQuery.ToArray();

            if (pointsSets.Length >= 2)
            {
                var terms = new List<Term>();
                foreach (var pair in pointsSets.SeqPairs())
                {
                    var fst = pair.Item1;
                    var snd = pair.Item2;

                    var p1 = fst.Center;
                    var p2 = snd.Center;

                    var n1 = fst.Axis;
                    var n2 = snd.Axis;

                    var pts1 = GetPointsOnPlane(p1, n1);
                    var pts2 = GetPointsOnPlane(p2, n2);

                    var planarity1 = PointsOnPlane(p1, n1, pts2);
                    var planarity2 = PointsOnPlane(p2, n2, pts1);

                    terms.Add(planarity1 + planarity2);
                }

                return TermUtils.SafeSum(terms);
            }
            else
                return 0;
        }

        private Term PointsOnPlane(TVec p, TVec n, IEnumerable<TVec> pts)
        {
            var terms =
                from x in pts
                let diff = p - x
                let numerator = diff * n
                let denominator = TermBuilder.Power(diff.NormSquared, -0.5)
                select TermBuilder.Power(numerator * denominator, 2);

            return TermUtils.SafeSum(terms);
        }

        private IEnumerable<TVec> GetPointsOnPlane(TVec p, TVec n)
        {
            var vec3d = random.NextVector3D().Normalized();
            var tvec = new TVec(vec3d.X, vec3d.Y, vec3d.Z);
            var t = TVec.CrossProduct(n, tvec);
            var u = TVec.CrossProduct(n, t);

            yield return p + t;
            yield return p + u;
            yield return p - t;
            yield return p - u;
        }

        private Term GetConcreteAnnotationTerm(Parallelism parallelism)
        {
            return 0; // meanwhile we don't handle parallelism
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

        private Dictionary<PointsSequence, CurveCategory> Categorize(
            Dictionary<PointsSequence, CurveCategory> inputWithInitialCategorization, 
            params CurveCategory[] categories)
        {
            // create viewmode/view for the categorizer window
            var categorizerVM = container.Resolve<CategorizerViewModel>();
            var categorizerView = container.Resolve<CategorizerView>(new DependencyOverride<CategorizerViewModel>(categorizerVM));

            // display the categorizer window with the correct data
            categorizerVM.Setup(inputWithInitialCategorization, categories);
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
