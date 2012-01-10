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
        private readonly SessionData sessionData;

        public OnSphereInferrer(SessionData sessionData)
        {
            this.sessionData = sessionData;
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
            var result = Enumerable.Empty<Annotation>();

            // we will take the list of all non-spheres, to test against the new sphere
            var snappedPrimitives = sessionData.SnappedPrimitives;
            var notSpheres = snappedPrimitives.Except(snappedPrimitives.OfType<SnappedSphere>());

            foreach (var snappedPrimitive in notSpheres)
            {
                var featureCurvesOnSphere = FindFeatureCurvesOnSphere(snappedPrimitive, toBeAnnotatedSphere);
                if (featureCurvesOnSphere.Any())
                    result = result.Concat(FeatureCurvesOnSphereAnnotation(featureCurvesOnSphere, toBeAnnotatedSphere));
            }

            return result;
        }

        private IEnumerable<Annotation> InferNonSphereAnnotations(SnappedPrimitive toBeAnnotated)
        {
            return Enumerable.Empty<Annotation>(); // TODO
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
            return dist < toBeAnnotatedSphere.RadiusResult;
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

    }
}
