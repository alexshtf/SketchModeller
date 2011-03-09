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
    public partial class NewSnapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly UiState uiState;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;
        private readonly Random random;

        [InjectionConstructor]
        public NewSnapper(
            SessionData sessionData,
            UiState uiState,
            ILoggerFacade logger,
            IUnityContainer container,
            IEventAggregator eventAggregator)
        {
            this.sessionData = sessionData;
            this.uiState = uiState;
            this.logger = logger;
            this.container = container;
            this.eventAggregator = eventAggregator;
            this.random = new Random();

            logger.Log("NewSnapper created", Category.Debug, Priority.None);
        }

        public void Snap()
        {
            var selectedPolylines = sessionData.SelectedSketchObjects.OfType<Polyline>().ToArray();
            var selectedPolygons = sessionData.SelectedSketchObjects.OfType<Polygon>().ToArray();
            var selectedCylinder = sessionData.SelectedNewPrimitives.OfType<NewCylinder>().FirstOrDefault();
            var selectedCone = sessionData.SelectedNewPrimitives.OfType<NewCone>().FirstOrDefault();

            if (selectedCylinder != null)
            {
                var snappedCylinder = CreateSnappedCylinder(selectedPolylines, selectedPolygons, selectedCylinder);
                sessionData.SnappedPrimitives.Add(snappedCylinder);
                sessionData.NewPrimitives.Remove(selectedCylinder);
            }
            if (selectedCone != null)
            {
                var snappedCone = CreateSnappedCone(selectedPolylines, selectedPolygons, selectedCone);
                sessionData.SnappedPrimitives.Add(snappedCone);
                sessionData.NewPrimitives.Remove(selectedCone);
            }

            OptimizeAll();

            eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
        }

        public void Recalculate()
        {
            if (sessionData.SnappedPrimitives.Count > 0)
            {
                OptimizeAll();
                eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
            }
        }

        private void OptimizeAll()
        {
            bool stop = false; // will be set to true by some stopping criterion
            int count = 0;
            while (!stop)
            {
                ReconstructSnappedPrimitives();
                EnforceConstraints();

                // meanwhile a stupid stopping criterion
                if (++count == 100)
                    stop = true;
            }

            #region Reconstruct geometry data from the optimized parameters

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

            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
            {
                var normal = MathUtils3D.NormalVector(snappedCone.AxisResult);
                var secondNormal = Vector3D.CrossProduct(normal, snappedCone.AxisResult);
                snappedCone.BottomCircle = CirclePoints(
                    snappedCone.BottomCenterResult,
                    normal,
                    secondNormal,
                    snappedCone.BottomRadiusResult,
                    50);
                snappedCone.TopCircle = CirclePoints(
                    snappedCone.TopCenterResult,
                    normal,
                    secondNormal,
                    snappedCone.TopRadiusResult,
                    50);
            }


            #endregion
        }

        private void EnforceConstraints()
        {
            // we do nothing if we have no annotations
            if (sessionData.Annotations.Count == 0)
                return;

            #region Write all variables and their current values to a vector

            var variablesWriter = new VariableVectorsWriter();
            var startVectorWriter = new VectorsWriter();

            // write cylinders
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                variablesWriter.Write(snappedCylinder);
                startVectorWriter.Write(snappedCylinder);
            }

            // write cones
            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
            {
                variablesWriter.Write(snappedCone);
                startVectorWriter.Write(snappedCone);
            }

            #endregion

            #region Perform optimization

            var variables = variablesWriter.ToArray();
            var values = startVectorWriter.ToArray();

            // mean squared error from current state is the similarity objective
            var similarityObjective = (1 / (double)values.Length) * TermUtils.SafeSum(
                from pair in variables.Zip(values)
                select TermBuilder.Power(pair.Item1 + (-pair.Item2), 2));

            // we assume feature curves of our primitives know which which curves they are snapped to!
            // we use that info to build annotations terms
            var annotationTerms = sessionData.Annotations.Select(x => GetAnnotationTerm(x));
            var annotationsObjective = (1 / (double)sessionData.Annotations.Count) * TermUtils.SafeSum(annotationTerms);

            var totalObjective = 1000 * similarityObjective + annotationsObjective;

            var dataLM = Optimizer.GetLMFuncs(totalObjective);
            var optimum = Optimizer.MinimizeLM(dataLM, variables, values);

            #endregion

            #region read data back from the optimized vector

            var resultReader = new VectorsReader(optimum);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
                resultReader.Read(snappedCylinder);

            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
                resultReader.Read(snappedCone);

            #endregion
        }

        private void ReconstructSnappedPrimitives()
        {
            foreach (var snappedPrim in sessionData.SnappedPrimitives)
            {
                snappedPrim.MatchClass<SnappedCylinder>(cyl => Reconstruct(cyl));
            }
        }

        #region supporting code


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
            throw new NotImplementedException();// we still don't handle parallelism
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

        #endregion
    }

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
            var selectedCone = sessionData.SelectedNewPrimitives.OfType<NewCone>().FirstOrDefault();

            if (selectedCylinder != null)
            {
                var snappedCylinder = SnapCylinder(selectedPolylines, selectedPolygons, selectedCylinder);
                sessionData.SnappedPrimitives.Add(snappedCylinder);
                sessionData.NewPrimitives.Remove(selectedCylinder);
            }
            if (selectedCone != null)
            {
                var snappedCone = SnapCone(selectedPolylines, selectedPolygons, selectedCone);
                sessionData.SnappedPrimitives.Add(snappedCone);
                sessionData.NewPrimitives.Remove(selectedCone);
            }
            OptimizeAll();

            eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
        }

        public void Recalculate()
        {
            if (sessionData.SnappedPrimitives.Count > 0)
            {
                OptimizeAll();
                eventAggregator.GetEvent<SnapCompleteEvent>().Publish(null);
            }
        }

        private void OptimizeAll()
        {
            #region Write data to variables / value vector

            var variablesWriter = new VariableVectorsWriter();
            var startVectorWriter = new VectorsWriter();
            // write cylinders
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

            // write cones
            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
            {
                variablesWriter
                    .Write(snappedCone.Axis)
                    .Write(snappedCone.BottomCenter)
                    .Write(snappedCone.Length)
                    .Write(snappedCone.TopRadius)
                    .Write(snappedCone.BottomRadius);
                startVectorWriter
                    .Write(snappedCone.AxisResult)
                    .Write(snappedCone.BottomCenterResult)
                    .Write(snappedCone.LengthResult)
                    .Write(snappedCone.TopRadiusResult)
                    .Write(snappedCone.BottomRadiusResult);
            }

            #endregion

            #region perform optimization

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

            #endregion
           
            #region read data back from the optimized vector

            var resultReader = new VectorsReader(minimizer);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                snappedCylinder.AxisResult = resultReader.ReadVector3D();
                snappedCylinder.BottomCenterResult = resultReader.ReadPoint3D();
                snappedCylinder.LengthResult = resultReader.ReadValue();
                snappedCylinder.RadiusResult = resultReader.ReadValue();
            }
            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
            {
                snappedCone.AxisResult = resultReader.ReadVector3D();
                snappedCone.BottomCenterResult = resultReader.ReadPoint3D();
                snappedCone.LengthResult = resultReader.ReadValue();
                snappedCone.TopRadiusResult = resultReader.ReadValue();
                snappedCone.BottomRadiusResult = resultReader.ReadValue();
            }

            #endregion

            #region Reconstruct geometry data from the optimized parameters

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

            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
            {
                var normal = MathUtils3D.NormalVector(snappedCone.AxisResult);
                var secondNormal = Vector3D.CrossProduct(normal, snappedCone.AxisResult);
                snappedCone.BottomCircle = CirclePoints(
                    snappedCone.BottomCenterResult,
                    normal,
                    secondNormal,
                    snappedCone.BottomRadiusResult,
                    50);
                snappedCone.TopCircle = CirclePoints(
                    snappedCone.TopCenterResult,
                    normal,
                    secondNormal,
                    snappedCone.TopRadiusResult,
                    50);
            }


            #endregion
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
    }
}
