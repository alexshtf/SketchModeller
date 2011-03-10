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
using System.Diagnostics;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly UiState uiState;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;
        private readonly Random random;
        private readonly SnappersManager snappersManager;

        [InjectionConstructor]
        public Snapper(
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

            snappersManager = new SnappersManager(uiState, sessionData);
            snappersManager.RegisterSnapper(new ConeSnapper());
            snappersManager.RegisterSnapper(new CylinderSnapper());

            logger.Log("NewSnapper created", Category.Debug, Priority.None);
        }

        public void Snap()
        {
            var selectedPolylines = sessionData.SelectedSketchObjects.OfType<Polyline>().ToArray();
            var selectedPolygons = sessionData.SelectedSketchObjects.OfType<Polygon>().ToArray();
            var selectedCylinder = sessionData.SelectedNewPrimitives.OfType<NewCylinder>().FirstOrDefault();
            var selectedCone = sessionData.SelectedNewPrimitives.OfType<NewCone>().FirstOrDefault();

            if (sessionData.SelectedNewPrimitives.Count == 1)
            {
                var newPrimitive = sessionData.SelectedNewPrimitives.First();
                var selectedCurves = sessionData.SelectedSketchObjects.ToArray();
                var snappedPrimitive = snappersManager.Create(selectedCurves, newPrimitive);
                sessionData.SnappedPrimitives.Add(snappedPrimitive);
                sessionData.NewPrimitives.Remove(newPrimitive);
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

            // all objective functions. Will be summed eventually to form one big objective.
            var objectives = new List<Term>();

            // all equality constraints.
            var constraints = new List<Term>();

            #region get objectives and constraints for primitives

            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
            {
                var objectiveAndConstraints = snappersManager.Reconstruct(snappedPrimitive);
                objectives.Add(objectiveAndConstraints.Item1);
                constraints.AddRange(objectiveAndConstraints.Item2);
            }
            
            #endregion

            #region get objectives and constraints for annotations

            foreach (var annotation in sessionData.Annotations)
            {
                var constraintTerms = GetAnnotationConstraints(annotation);
                constraints.AddRange(constraintTerms);
            }

            #endregion

            #region perform optimization

            var finalObjective = TermUtils.SafeSum(objectives);
            var vars = variablesWriter.ToArray();
            var vals = startVectorWriter.ToArray();

            var optimum = Optimizer.MinAugmentedLagrangian(finalObjective, constraints.ToArray(), vars, vals, mu:10);

            #endregion

            #region read data back from the optimized vector

            var resultReader = new VectorsReader(optimum);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
                resultReader.Read(snappedCylinder);

            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
                resultReader.Read(snappedCone);

            #endregion

            #region Reconstruct geometry data from the optimized parameters

            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
            {
                var normal = MathUtils3D.NormalVector(snappedCylinder.AxisResult);
                var secondNormal = Vector3D.CrossProduct(normal, snappedCylinder.AxisResult);
                snappedCylinder.BottomCircle = SnapperHelper.CirclePoints(
                    snappedCylinder.BottomCenterResult,
                    normal,
                    secondNormal,
                    snappedCylinder.RadiusResult,
                    50);

                snappedCylinder.TopCircle = SnapperHelper.CirclePoints(
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
                snappedCone.BottomCircle = SnapperHelper.CirclePoints(
                    snappedCone.BottomCenterResult,
                    normal,
                    secondNormal,
                    snappedCone.BottomRadiusResult,
                    50);
                snappedCone.TopCircle = SnapperHelper.CirclePoints(
                    snappedCone.TopCenterResult,
                    normal,
                    secondNormal,
                    snappedCone.TopRadiusResult,
                    50);
            }


            #endregion
        }

        #region supporting code

        private Term[] GetAnnotationConstraints(Annotation x)
        {
            var constraints = new Term[0];
            x.MatchClass<Parallelism>(parallelism => constraints = GetConcreteAnnotationTerm(parallelism));
            x.MatchClass<Coplanarity>(coplanarity => constraints = GetConcreteAnnotationTerm(coplanarity));
            return constraints;
        }

        private Term[] GetConcreteAnnotationTerm(Coplanarity coplanarity)
        {
            var pointsSetsQuery =
                from curve in coplanarity.Elements
                from snappedPrimitive in sessionData.SnappedPrimitives
                from pointsSet in snappedPrimitive.SnappedPointsSets
                where pointsSet.SnappedTo == curve
                select pointsSet;

            var pointsSets = pointsSetsQuery.ToArray();
            var constraints = new List<Term>();

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

                    var planarity1 = PointsOnPlaneConstraint(p1, n1, pts2);
                    var planarity2 = PointsOnPlaneConstraint(p2, n2, pts1);
                    constraints.AddRange(planarity1);
                    constraints.AddRange(planarity2);
                }
            }
            return constraints.ToArray();
        }

        private Term[] PointsOnPlaneConstraint(TVec p, TVec n, IEnumerable<TVec> pts)
        {
            var constraints =
                from x in pts
                let diff = p - x
                select diff * n;
            return constraints.ToArray();
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

        private Term[] GetConcreteAnnotationTerm(Parallelism parallelism)
        {
            throw new NotImplementedException();// we still don't handle parallelism
        }

        #endregion
    }
}
