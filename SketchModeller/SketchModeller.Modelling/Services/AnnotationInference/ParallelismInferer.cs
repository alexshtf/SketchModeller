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
    class ParallelismInferer: IInferrer
    {
        public const double DEFAULT_ANGLE_THRESHOLD = 20 * Math.PI / 180;

        private SessionData sessionData;
        private double angleThreshold;

        public ParallelismInferer(SessionData sessionData, 
                                  double angleThreshold = DEFAULT_ANGLE_THRESHOLD)
        {
            this.sessionData = sessionData;
            this.angleThreshold = angleThreshold;
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

                Annotation parallelism = new Parallelism { Elements = new FeatureCurve[] { newFeatureCurve, existingFeatureCurve } };

                return UtilsEnumerable.Singleton(parallelism);
            }
            else
                return Enumerable.Empty<Annotation>();
        }

        private bool AreGoodCandidates(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            bool angleCondition = IsAngleBelowThreshold(firstCurve, secondCurve);
            return angleCondition;
        }

        private bool IsAngleBelowThreshold(FeatureCurve firstCurve, FeatureCurve secondCurve)
        {
            var crossLength = Vector3D.CrossProduct(firstCurve.NormalResult.Normalized(), secondCurve.NormalResult.Normalized()).Length;
            var angle = Math.Asin(crossLength);
            return angle <= angleThreshold;
        }

        private double DistanceBetweenCurves(FeatureCurve first, FeatureCurve second)
        {
            return (first.CenterResult - second.CenterResult).Length;
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
