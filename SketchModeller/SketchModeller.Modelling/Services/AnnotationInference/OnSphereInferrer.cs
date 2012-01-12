using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using System.Windows;
using Utils;
using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class OnSphereInferrer : IInferrer
    {
        private const double DEFAULT_PROXIMITY_THRESHOLD_FRACTION = 0.1; // 10%

        private readonly SessionData sessionData;
        private readonly double proximityThresholdFraction;

        public OnSphereInferrer(SessionData sessionData, double proximityThresholdFraction = DEFAULT_PROXIMITY_THRESHOLD_FRACTION)
        {
            this.sessionData = sessionData;
            this.proximityThresholdFraction = proximityThresholdFraction;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            var toBeAnnotatedSphere = toBeAnnotated as SnappedSphere;
            if (toBeAnnotatedSphere == null)
                return InferNonSphereAnnotations(toBeAnnotated);
            else
                return InferSphereAnnotations(toBeAnnotatedSphere);
        }

        private IEnumerable<Annotation> InferSphereAnnotations(SnappedSphere toBeAnnotatedSphere)
        {
            // we will take the list of all non-spheres, to test against the new sphere
            var snappedPrimitives = sessionData.SnappedPrimitives;
            var notSpheres = snappedPrimitives.Except(snappedPrimitives.OfType<SnappedSphere>());

            return from notSphere in notSpheres
                   from constraint in GetConstraintsForSphereWithNonspherePair(notSphere, toBeAnnotatedSphere)
                   select constraint;
        }

        private IEnumerable<Annotation> InferNonSphereAnnotations(SnappedPrimitive toBeAnnotated)
        {
            var spheres = sessionData.SnappedPrimitives.OfType<SnappedSphere>();
            return from sphere in spheres
                   from constraint in GetConstraintsForSphereWithNonspherePair(toBeAnnotated, sphere)
                   select constraint;
        }

        private IEnumerable<Annotation> GetConstraintsForSphereWithNonspherePair(SnappedPrimitive nonSphere, SnappedSphere sphere)
        {
            var featureCurvesOnSphere = FindFeatureCurvesOnSphere(nonSphere, sphere);
            if (featureCurvesOnSphere.Any())
                return FeatureCurvesOnSphereAnnotation(featureCurvesOnSphere, sphere);

            var hasSilhouettesWithEndpointsOnSphere = HasSilhouettesWithEndpointsOnSphere(nonSphere, sphere);
            if (hasSilhouettesWithEndpointsOnSphere)
                return SilhouettesWithEndpointsOnSphereAnnotation(nonSphere, sphere);

            return Enumerable.Empty<Annotation>();
        }

        private bool HasSilhouettesWithEndpointsOnSphere(SnappedPrimitive nonSphere, SnappedSphere sphere)
        {
            var sketchSilhouettes = from pointsSequence in nonSphere.SnappedTo
                                    where pointsSequence.CurveCategory == CurveCategories.Silhouette
                                    where HasEndpointOnSphere(pointsSequence, sphere)
                                    select pointsSequence;
            return sketchSilhouettes.Any();
        }

        private IEnumerable<Annotation> SilhouettesWithEndpointsOnSphereAnnotation(SnappedPrimitive nonSphere, SnappedSphere sphere)
        {
            var unsnappedFeatureCurves = from featureCurve in nonSphere.FeatureCurves
                                         where featureCurve.SnappedTo == null
                                         select featureCurve;
            var firstUnsnapped = unsnappedFeatureCurves.First();

            yield return new OnSphere
            {
                CenterTouchesSphere = firstUnsnapped,
                SphereOwned = sphere.ProjectionParallelCircle,
                Elements = Utils.Enumerable.ArrayOf(firstUnsnapped, sphere.ProjectionParallelCircle),
            };
        }

        private bool HasEndpointOnSphere(PointsSequence pointsSequence, SnappedSphere sphere)
        {
            var isStartInsideSphere = IsInsideSphere(pointsSequence.Points.First(), sphere);
            var isEndInsideSphere = IsInsideSphere(pointsSequence.Points.Last(), sphere);
            return isStartInsideSphere || isEndInsideSphere;
        }

        private IEnumerable<Annotation> FeatureCurvesOnSphereAnnotation(IEnumerable<FeatureCurve> featureCurvesOnSphere, SnappedSphere toBeAnnotatedSphere)
        {
            var farthestFeatureCurve = featureCurvesOnSphere.Minimizer(fc => -fc.CenterResult.Z); // maximal Z coordinate
            yield return new OnSphere
            {
                CenterTouchesSphere = farthestFeatureCurve,
                SphereOwned = toBeAnnotatedSphere.ProjectionParallelCircle,
                Elements = new FeatureCurve[] { farthestFeatureCurve, toBeAnnotatedSphere.ProjectionParallelCircle }
            };
        }

        private IEnumerable<FeatureCurve> FindFeatureCurvesOnSphere(SnappedPrimitive snappedPrimitive, SnappedSphere toBeAnnotatedSphere)
        {
            var snappedFeatureCurvesInsideSphere = from curve in snappedPrimitive.FeatureCurves
                                                   where curve.SnappedTo != null
                                                   where IsInsideSphere(curve, toBeAnnotatedSphere)
                                                   select curve;
            return snappedFeatureCurvesInsideSphere.ToArray();
        }

        private bool IsInsideSphere(FeatureCurve curve, SnappedSphere toBeAnnotatedSphere)
        {
            var insideSpherePointsCount = curve.SnappedTo.Points.Count(p => IsInsideSphere(p, toBeAnnotatedSphere));
            if (insideSpherePointsCount > 0.5 * curve.SnappedTo.Points.Length) // at-least half of the points are inside the sphere
                return true;
            else
                return false;
        }

        private bool IsInsideSphere(Point p, SnappedSphere toBeAnnotatedSphere)
        {
            var sphereCenter = new Point(toBeAnnotatedSphere.CenterResult.X, -toBeAnnotatedSphere.CenterResult.Y);
            var dist = (p - sphereCenter).Length;
            return dist < (1 + proximityThresholdFraction) * toBeAnnotatedSphere.RadiusResult;
        }

    }
}
