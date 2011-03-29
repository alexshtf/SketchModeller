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
                // initialize our snapped primitive
                var newPrimitive = sessionData.SelectedNewPrimitives.First();
                var selectedCurves = sessionData.SelectedSketchObjects.ToArray();
                var snappedPrimitive = snappersManager.Create(selectedCurves, newPrimitive);
                snappedPrimitive.UpdateFeatureCurves();

                // update session data
                sessionData.SnappedPrimitives.Add(snappedPrimitive);
                sessionData.NewPrimitives.Remove(newPrimitive);
                sessionData.FeatureCurves.AddRange(snappedPrimitive.FeatureCurves);
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

            var curvesToAnnotations = new Dictionary<FeatureCurve, ISet<Annotation>>();

            #region Get mapping of curves to annotations

            foreach (var fc in sessionData.FeatureCurves)
                curvesToAnnotations[fc] = new HashSet<Annotation>();

            foreach (var annotation in sessionData.Annotations)
            {
                IEnumerable<FeatureCurve> curves = null;
                annotation.MatchClass<Parallelism>(pa => curves = pa.Elements);
                annotation.MatchClass<Coplanarity>(ca => curves = ca.Elements);
                annotation.MatchClass<Cocentrality>(ca => curves = ca.Elements);
                Debug.Assert(curves != null);
                foreach (var fc in curves)
                    curvesToAnnotations[fc].Add(annotation);
            }

            #endregion

            #region get objectives and constraints for primitives

            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
            {
                var objectiveAndConstraints = snappersManager.Reconstruct(snappedPrimitive, curvesToAnnotations);
                objectives.Add(objectiveAndConstraints.Item1);
                constraints.AddRange(objectiveAndConstraints.Item2);
            }
            
            #endregion

            #region get constraints for annotations

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

            var optimum = Optimizer.MinAugmentedLagrangian(finalObjective, constraints.ToArray(), vars, vals, mu:10, tolerance:1E-5);

            #endregion

            #region read data back from the optimized vector

            var resultReader = new VectorsReader(optimum);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
                resultReader.Read(snappedCylinder);

            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
                resultReader.Read(snappedCone);

            #endregion

            #region Update feature curves

            foreach (var snappedPrimitive in sessionData.SnappedPrimitives)
                snappedPrimitive.UpdateFeatureCurves();

            #endregion
        }

        #region supporting code

        private Term[] GetAnnotationConstraints(Annotation x)
        {
            var constraints = new Term[0];
            x.MatchClass<Parallelism>(parallelism => constraints = GetConcreteAnnotationTerm(parallelism));
            x.MatchClass<Coplanarity>(coplanarity => constraints = GetConcreteAnnotationTerm(coplanarity));
            x.MatchClass<Cocentrality>(cocentrality => constraints = GetConcreteAnnotationTerm(cocentrality));
            return constraints;
        }

        private Term[] GetConcreteAnnotationTerm(Cocentrality cocentrality)
        {
            var constraints = new List<Term>();
            if (cocentrality.Elements.Length >= 2)
            {
                foreach (var pair in cocentrality.Elements.SeqPairs())
                {
                    var fc1 = pair.Item1;
                    var fc2 = pair.Item2;
                    constraints.Add(fc1.Center.X - fc2.Center.X);
                    constraints.Add(fc1.Center.Y - fc2.Center.Y);
                    constraints.Add(fc1.Center.Z - fc2.Center.Z);
                }
            }

            return constraints.ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(Coplanarity coplanarity)
        {
            var constraints = new List<Term>();

            if (coplanarity.Elements.Length >= 2)
            {
                var terms = new List<Term>();
                foreach (var pair in coplanarity.Elements.SeqPairs())
                {
                    var fst = pair.Item1;
                    var snd = pair.Item2;

                    var p1 = fst.Center;
                    var p2 = snd.Center;

                    var n1 = fst.Normal;
                    var n2 = snd.Normal;

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
