using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using Utils;

using Enumerable = System.Linq.Enumerable;
using UtilsEnumerable = Utils.Enumerable;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class OrthogonalityInferrer : IInferrer
    {
        public const double DEFAULT_ANGLE_THRESHOLD = 20 * Math.PI / 180;
        public const double DEFAULT_PROXIMITY_THRESHOLD = 0.4;

        private SessionData sessionData;
        private double angleThreshold;
        private double proximityThreshold;

        public OrthogonalityInferrer(SessionData sessionData, double angleThreshold = DEFAULT_ANGLE_THRESHOLD, double proximityThreshold = DEFAULT_PROXIMITY_THRESHOLD)
        {
            this.sessionData = sessionData;
            this.angleThreshold = angleThreshold;
            this.proximityThreshold = proximityThreshold;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            // we skip orthogonality inference for spheres
            if (toBeSnapped is NewSphere)
                return Enumerable.Empty<Annotation>();

            var curvesToSkip = toBeAnnotated.FeatureCurves.Concat(GetSphereFeatureCurves()).ToArray();

            var candidates = 
                from firstCurve in toBeAnnotated.FeatureCurves
                from secondCurve in sessionData.FeatureCurves.Except(curvesToSkip)
                where AreGoodCandidates(firstCurve, secondCurve)
                select Tuple.Create(firstCurve, secondCurve);

            if (candidates.Any())
            {
                var bestCandidate = candidates.Minimizer(pair => DistanceBetweenCurves(pair.Item1, pair.Item2));
                var newFeatureCurve = bestCandidate.Item1;
                var existingFeatureCurve = bestCandidate.Item2;

                Annotation curveOrthogonality = new OrthogonalAxis 
                { 
                    Elements = new FeatureCurve[] { newFeatureCurve, existingFeatureCurve } 
                };
                Annotation coplanarCenters = new CoplanarCenters
                {
                    Elements = AllCurvesOnPrimitiveOf(existingFeatureCurve).Append(newFeatureCurve).ToArray()
                };

                return UtilsEnumerable.ArrayOf(curveOrthogonality, coplanarCenters);
            }
            else
                return Enumerable.Empty<Annotation>();
        }

        private IEnumerable<FeatureCurve> GetSphereFeatureCurves()
        {
            return from primitive in sessionData.SnappedPrimitives
                   where primitive is SnappedSphere
                   from curve in primitive.FeatureCurves
                   select curve;
        }

        private IEnumerable<FeatureCurve> AllCurvesOnPrimitiveOf(FeatureCurve featureCurve)
        {
            var containingPrimitive = from primitive in sessionData.SnappedPrimitives
                                      where primitive.FeatureCurves.Contains(featureCurve)
                                      select primitive;
            return containingPrimitive.First().FeatureCurves;
        }

        private bool AreGoodCandidates(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            bool angleCondition = IsAngleBelowThreshold(firstCurve, secondCurve);
            //bool proximityCondition = IsProximityBelowThreshold(firstCurve, secondCurve);
            bool proximityCondition = true;
            bool result = angleCondition && proximityCondition;
            return result;
        }

        private bool IsProximityBelowThreshold(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            var distance = DistanceBetweenCurves(firstCurve, secondCurve);
            return distance <= proximityThreshold;
        }

        private bool IsAngleBelowThreshold(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            double dot = Vector3D.DotProduct(firstCurve.NormalResult, secondCurve.NormalResult);
            double angle = Math.Acos(dot);
            double angleDiff = Math.Abs(Math.PI / 2 - angle);
            return angleDiff <= angleThreshold;
        }

        private double DistanceBetweenCurves(FeatureCurve first, FeatureCurve second)
        {
            return (first.CenterResult - second.CenterResult).Length;
        }
    }
}
