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

        public Snapper(SessionData sessionData, ILoggerFacade logger, IEventAggregator eventAggregator, IUnityContainer container)
        {
            this.sessionData = sessionData;
            this.logger = logger;
            this.container = container;
            this.eventAggregator = eventAggregator;
        }

        public void Snap()
        {
            var selectedPolylines =
                (from polyline in sessionData.SketchObjects.OfType<Polyline>()
                 where polyline.IsSelected == true
                 select polyline
                ).ToArray();

            var selectedPolygons =
                (from polygon in sessionData.SketchObjects.OfType<Polygon>()
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
                    .Write(snappedCylinder.AxisNormal)
                    .Write(snappedCylinder.BottomCenter)
                    .Write(snappedCylinder.Length)
                    .Write(snappedCylinder.Radius);
                startVectorWriter
                    .Write(snappedCylinder.AxisResult)
                    .Write(snappedCylinder.AxisNormalResult)
                    .Write(snappedCylinder.BottomCenterResult)
                    .Write(snappedCylinder.LengthResult)
                    .Write(snappedCylinder.RadiusResult);
            }

            var dataTerms = sessionData.SnappedPrimitives.Select(p => p.DataTerm);
            var annotationTerms = sessionData.Annotations.Select(x => GetAnnotationTerm(x));
            var dataObjective = TermUtils.SafeSum(dataTerms);
            var annotationsObjective = TermUtils.SafeSum(annotationTerms);

            var variables = variablesWriter.ToArray();
            var startVector = startVectorWriter.ToArray();
            
            var minimizer = startVector;
            for (int i = 0; i < 5; ++i)
            {
                minimizer = Optimizer.Minimize(annotationsObjective, variables, minimizer);
                minimizer = Optimizer.Minimize(dataObjective, variables, minimizer);
            }

            var finalTerm = TermUtils.SafeSum(dataTerms.Concat(annotationTerms));
            minimizer = Optimizer.Minimize(finalTerm, variables, minimizer);
            
            var logMsg = string.Format("Value at start vector is {0}, value at minimizer is {1}",
                Evaluator.Evaluate(finalTerm, variablesWriter.ToArray(), startVector),
                Evaluator.Evaluate(finalTerm, variablesWriter.ToArray(), minimizer));
            logger.Log(logMsg, Category.Info, Priority.None);

            var resultReader = new VectorsReader(minimizer);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                snappedCylinder.AxisResult = resultReader.ReadVector3D();
                snappedCylinder.AxisNormalResult = resultReader.ReadVector3D();
                snappedCylinder.BottomCenterResult = resultReader.ReadPoint3D();
                snappedCylinder.LengthResult = resultReader.ReadValue();
                snappedCylinder.RadiusResult = resultReader.ReadValue();
            }

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                var secondNormal = Vector3D.CrossProduct(snappedCylinder.AxisNormalResult, snappedCylinder.AxisResult);
                snappedCylinder.BottomCircle = CirclePoints(
                    snappedCylinder.BottomCenterResult,
                    snappedCylinder.AxisNormalResult,
                    secondNormal,
                    snappedCylinder.RadiusResult,
                    20);

                snappedCylinder.TopCircle = CirclePoints(
                    snappedCylinder.TopCenterResult,
                    snappedCylinder.AxisNormalResult,
                    secondNormal,
                    snappedCylinder.RadiusResult,
                    20);
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
                select pointsSet.PointTerms;

            var pointsSets = new HashSet<ReadOnlyCollection<TVec>>(pointsSetsQuery);
            if (pointsSets.Count < 2)
                return 0;
            else
            {
                var outerSumTerms = new List<Term>();
                foreach (var pair in pointsSets.SeqPairs())
                {
                    var fstSet = pair.Item1;
                    var sndSet = pair.Item2;
                    var count = Math.Max(fstSet.Count, sndSet.Count);
                    var innerSumTerms = new Term[count];
                    for (int i = 0; i < count; ++i)
                    {
                        var p1 = fstSet[i % fstSet.Count];
                        var p2 = fstSet[(i + 1) % fstSet.Count];

                        var q1 = sndSet[i % sndSet.Count];
                        var q2 = sndSet[(i + 1) % sndSet.Count];

                        innerSumTerms[i] = TermBuilder.Power(GeometricTests.Coplanarity3D(p1, p2, q1, q2), 2);
                    }
                    var innerTerm = (1 / (double)count) * TermUtils.SafeSum(innerSumTerms);
                    outerSumTerms.Add(innerTerm);
                }
                return TermUtils.SafeSum(outerSumTerms);
            }
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
