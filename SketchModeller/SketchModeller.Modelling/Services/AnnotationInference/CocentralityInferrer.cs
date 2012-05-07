using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using Utils;

using Enumerable = System.Linq.Enumerable;
using System.Windows;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class CocentralityInferrer : IInferrer
    {
        public const double DEFAULT_ABSOLUTE_THRESHOLD = 0.05;
        public const double DEFAULT_RELATIVE_THRESHOLD = 0.1; // 10%

        private readonly SessionData sessionData;
        private readonly double relativeThreshold;
        private readonly double absoluteThreshold;

        /// <summary>
        /// Constructs a new instance of CocentralityInferrer
        /// </summary>
        /// <param name="sessionData"></param>
        /// <param name="relativeThreshold"></param>
        /// <param name="absoluteThreshold"></param>
        public CocentralityInferrer(
            SessionData sessionData, 
            double relativeThreshold = DEFAULT_RELATIVE_THRESHOLD, 
            double absoluteThreshold = DEFAULT_ABSOLUTE_THRESHOLD)
        {
            this.sessionData = sessionData;
            this.relativeThreshold = relativeThreshold;
            this.absoluteThreshold = absoluteThreshold;
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
                group Tuple.Create(firstCurve, secondCurve) by firstCurve;

            return from candidatesGroup in candidates
                   let bestCandidate = candidatesGroup.Minimizer(pair => CurveDistance(pair.Item1, pair.Item2))
                   select new Cocentrality
                   {
                       Elements = new FeatureCurve[] { bestCandidate.Item1, bestCandidate.Item2 }
                   };
        }

        private bool AreGoodCandidates(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            bool areRelativeClose = AreRelativeClose(firstCurve, secondCurve);
            bool areAbsoluteClose = AreAbsoluteClose(firstCurve, secondCurve);
            return areRelativeClose || areAbsoluteClose;
        }

        private bool AreAbsoluteClose(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            var dist = CurveDistance(firstCurve, secondCurve);
            return dist < absoluteThreshold;
        }

        private bool AreRelativeClose(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            var firstCircle = firstCurve as CircleFeatureCurve;
            var secondCircle = secondCurve as CircleFeatureCurve;

            if (firstCircle == null || secondCircle == null)
                return false;

            var radiiSum = firstCircle.RadiusResult + secondCircle.RadiusResult;
            var dist = CurveDistance(firstCurve, secondCurve);

            if (radiiSum == 0)
                return false;
            else
                return (dist / radiiSum) <= relativeThreshold;
        }

        private double CurveDistance(FeatureCurve curve1, FeatureCurve curve2)
        {
            if (curve1.IsSameObjectCurve(curve2))
                return 0;
            else
                return DistanceBetweenCenters(curve1, curve2);
        }

        private double DistanceBetweenCenters(FeatureCurve curve1, FeatureCurve curve2)
        {
            var diff = curve1.CenterResult - curve2.CenterResult;
            var dist3d = diff.Length;
            var dist2d = new Vector(diff.X, diff.Y).Length;

            return Math.Min(dist3d, dist2d);
        }

        private IEnumerable<FeatureCurve> GetSphereFeatureCurves()
        {
            return from primitive in sessionData.SnappedPrimitives
                   where primitive is SnappedSphere
                   from curve in primitive.FeatureCurves
                   select curve;
        }
    }
}
