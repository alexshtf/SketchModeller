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
using System.Windows;
using TermUtils = SketchModeller.Utilities.TermUtils;
using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Services.Snap
{
    public partial class Snapper : ISnapper
    {
        private readonly SessionData sessionData;
        private readonly UiState uiState;
        private readonly ILoggerFacade logger;
        private readonly IUnityContainer container;
        private readonly IEventAggregator eventAggregator;
        private readonly SnappersManager snappersManager;
        private readonly IAnnotationInference annotationInference;

        [InjectionConstructor]
        public Snapper(
            SessionData sessionData,
            UiState uiState,
            ILoggerFacade logger,
            IUnityContainer container,
            IEventAggregator eventAggregator,
            IAnnotationInference annotationInference)
        {
            this.sessionData = sessionData;
            this.uiState = uiState;
            this.logger = logger;
            this.container = container;
            this.eventAggregator = eventAggregator;
            this.annotationInference = annotationInference;

            snappersManager = new SnappersManager(uiState, sessionData);
            snappersManager.RegisterSnapper(new ConeSnapper());
            snappersManager.RegisterSnapper(new CylinderSnapper());
            snappersManager.RegisterSnapper(new SphereSnapper());
            snappersManager.RegisterSnapper(new SgcSnapper());
            snappersManager.RegisterSnapper(new BgcSnapper());

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

                // update annotations with the inferred ones
                var annotations = annotationInference.InferAnnotations(newPrimitive, snappedPrimitive);
                sessionData.Annotations.AddRange(annotations);
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

            //MessageBox.Show("Inside Optimize All");
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

            // write spheres
            foreach (var snappedSphere in sessionData.SnappedPrimitives.OfType<SnappedSphere>())
            {
                variablesWriter.Write(snappedSphere);
                startVectorWriter.Write(snappedSphere);
            }

            foreach (var snappedSgc in sessionData.SnappedPrimitives.OfType<SnappedStraightGenCylinder>())
            {
                variablesWriter.Write(snappedSgc);
                startVectorWriter.Write(snappedSgc);
            }

            foreach (var snappedBgc in sessionData.SnappedPrimitives.OfType<SnappedBendedGenCylinder>())
            {
                //MessageBox.Show("Writting Bgc");
                variablesWriter.Write(snappedBgc);
                startVectorWriter.Write(snappedBgc);
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
                IEnumerable<FeatureCurve> curves = annotation.Elements;
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
            MessageBox.Show("Performing Optimization");
            #region perform optimization

            var finalObjective = TermUtils.SafeSum(objectives);
            var vars = variablesWriter.ToArray();
            var vals = startVectorWriter.ToArray();

            //var optimum = Optimizer.MinAugmentedLagrangian(finalObjective, constraints.ToArray(), vars, vals, mu:10, tolerance:1E-5);
            var optimum = ALBFGSOptimizer.Minimize(
                finalObjective, constraints.ToArray(), vars, vals, mu: 10, tolerance: 1E-5);
            MessageBox.Show("Ended Optimization");
            #endregion

            #region read data back from the optimized vector

            var resultReader = new VectorsReader(optimum);
            foreach (var snappedCylinder in sessionData.SnappedPrimitives.OfType<SnappedCylinder>())
                resultReader.Read(snappedCylinder);

            foreach (var snappedCone in sessionData.SnappedPrimitives.OfType<SnappedCone>())
                resultReader.Read(snappedCone);

            foreach (var snappedSphere in sessionData.SnappedPrimitives.OfType<SnappedSphere>())
                resultReader.Read(snappedSphere);

            foreach (var snappedSgc in sessionData.SnappedPrimitives.OfType<SnappedStraightGenCylinder>())
                resultReader.Read(snappedSgc);

            foreach (var snappedBgc in sessionData.SnappedPrimitives.OfType<SnappedBendedGenCylinder>())
            {
                MessageBox.Show("Reading Variables....");
                resultReader.Read(snappedBgc);
                //MessageBox.Show("Done Reading Variables....");
            }
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
            x.MatchClass<ColinearCenters>(colinearCenters => constraints = GetConcreteAnnotationTerm(colinearCenters));
            x.MatchClass<CoplanarCenters>(coplanarCenters => constraints = GetConcreteAnnotationTerm(coplanarCenters));
            x.MatchClass<OrthogonalAxis>(orthogonalAxes => constraints = GetConcreteAnnotationTerm(orthogonalAxes));
            return constraints;
        }

        private Term[] GetConcreteAnnotationTerm(OrthogonalAxis orthoonalAxes)
        {
            if (orthoonalAxes.Elements.Length != 2)
                return Enumerable.Empty<Term>().ToArray();

            var firstNormal = orthoonalAxes.Elements[0].Normal;
            var secondNormal = orthoonalAxes.Elements[1].Normal;
            var innerProduct = firstNormal * secondNormal;

            return new Term[] { innerProduct };
        }

        private Term[] GetConcreteAnnotationTerm(CoplanarCenters coplanarCenters)
        {
            var constraints = new List<Term>();
            if (coplanarCenters.Elements.Length >= 2)
            {
                foreach (var pair in coplanarCenters.Elements.SeqPairs())
                {
                    var c1 = pair.Item1.Center;
                    var n1 = pair.Item1.Normal;
                    var c2 = pair.Item2.Center;
                    var n2 = pair.Item2.Normal;

                    var term = (c2 - c1) * TVec.CrossProduct(n1, n2);
                    constraints.Add(term);
                }
            }

            return constraints.ToArray();
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

                    constraints.AddRange(VectorParallelism(n1, n2));
                    constraints.AddRange(PointsOnPlaneConstraint(p1, n1, new TVec[] { p2 }));
                    constraints.AddRange(PointsOnPlaneConstraint(p2, n2, new TVec[] { p1 }));
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


        private Term[] GetConcreteAnnotationTerm(Parallelism parallelism)
        {
            var terms = new List<Term>();
            if (parallelism.Elements.Length >= 2)
            {
                var normals = from elem in parallelism.Elements
                              select elem.Normal;

                foreach (var normalsPair in normals.SeqPairs())
                {
                    var n1 = normalsPair.Item1;
                    var n2 = normalsPair.Item2;

                    terms.AddRange(VectorParallelism(n1, n2));
                }
            }
            return terms.ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(ColinearCenters colinearCenters)
        {
            var terms = new List<Term>();
            if (colinearCenters.Elements.Length >= 3)
            {
                var centers = from elem in colinearCenters.Elements
                              select elem.Center;

                foreach (var triple in centers.SeqTripples())
                {
                    var u = triple.Item1 - triple.Item2;
                    var v = triple.Item2 - triple.Item3;
                    
                    terms.AddRange(VectorParallelism(u, v));
                }
            }
            return terms.ToArray();
        }

        private static IEnumerable<Term> VectorParallelism(TVec u, TVec v)
        {
            yield return u.X * v.Y - v.X * u.Y;
            yield return u.Y * v.Z - u.Z * v.Y;
            yield return u.X * v.Z - v.X * u.Z;
        }

        #endregion
    }
}
