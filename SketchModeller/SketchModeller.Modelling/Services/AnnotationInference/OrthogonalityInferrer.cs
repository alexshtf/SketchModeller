using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using Utils;

using Enumerable = System.Linq.Enumerable;
using System.Windows.Media.Media3D;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class OrthogonalityInferrer : IInferrer
    {
        public const double THIRTY_DEGREES = 30 * Math.PI / 180;
        public const double DEFAULT_PROXIMITY_THRESHOLD = 0.4;

        private SessionData sessionData;
        private double angleThreshold;
        private double proximityThreshold;

        public OrthogonalityInferrer(SessionData sessionData, double angleThreshold = THIRTY_DEGREES, double proximityThreshold = DEFAULT_PROXIMITY_THRESHOLD)
        {
            this.sessionData = sessionData;
            this.angleThreshold = angleThreshold;
            this.proximityThreshold = proximityThreshold;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            var candidates = 
                from firstCurve in toBeAnnotated.FeatureCurves
                from secondCurve in sessionData.FeatureCurves.Except(toBeAnnotated.FeatureCurves)
                where AreGoodCandidates(firstCurve, secondCurve)
                select Tuple.Create(firstCurve, secondCurve);

            if (candidates.Any())
            {
                var bestCandidate = candidates.Minimizer(pair => DistanceBetweenCurves(pair.Item1, pair.Item2));
                var annotation = new OrthogonalAxis 
                { 
                    Elements = new FeatureCurve[] { bestCandidate.Item1, bestCandidate.Item2 } 
                };
                return Enumerable.Repeat(annotation, 1);
            }
                return Enumerable.Empty<Annotation>();
        }

        private bool AreGoodCandidates(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            bool angleCondition = IsAngleBelowThreshold(firstCurve, secondCurve);
            bool proximityCondition = IsProximityBelowThreshold(firstCurve, secondCurve);
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
