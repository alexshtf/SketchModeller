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
    class ColinearCentersInferer : IInferrer
    {
        private const double DEFAULT_PROXIMITY_THRESHOLD = 0.6;
        private const double DEFAULT_COLINEARITY_THRESHOLD = 10 * Math.PI / 180; // 10 degrees angle to be considered "colinear"
        private const double DEFAULT_PARALLELISM_THRSHOLD = 5 * Math.PI / 180; // 5 degrees angle between vectors to be considered "parallel"

        private SessionData sessionData;
        private double proximityThreshold;
        private double colinearityThreshold;
        private double parallelismThreshold;

        public ColinearCentersInferer(SessionData sessionData, 
                                      double proximityThreshold = DEFAULT_PROXIMITY_THRESHOLD,
                                      double colinearityThreshold = DEFAULT_COLINEARITY_THRESHOLD,
                                      double parallelismThreshold = DEFAULT_PARALLELISM_THRSHOLD)
        {
            this.sessionData = sessionData;
            this.proximityThreshold = proximityThreshold;
            this.colinearityThreshold = colinearityThreshold;
            this.parallelismThreshold = parallelismThreshold;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            var toBeAnnotatedCurves = toBeAnnotated.FeatureCurves;
            var candidateTriples =
                from i in Enumerable.Range(0, toBeAnnotatedCurves.Length)
                from j in Enumerable.Range(i + 1, toBeAnnotatedCurves.Length - i - 1)
                // at this point (i, j) are all the possible pairs of curves without repetitions
                let allExistingCurves = sessionData.FeatureCurves.Except(toBeAnnotatedCurves)
                from existingCurve in allExistingCurves
                where AreGoodCandidates(toBeAnnotatedCurves[i], toBeAnnotatedCurves[j], existingCurve)
                select new 
                { 
                    FistNewCurve = toBeAnnotatedCurves[i], 
                    SecondNewCurve = toBeAnnotatedCurves[j], 
                    ExistingCurve = existingCurve 
                };

            if (candidateTriples.Any())
            {
                var bestCandidate = candidateTriples.Minimizer(triple => ProximityMeasure(triple.FistNewCurve, triple.SecondNewCurve, triple.ExistingCurve));
                var annotation = new ColinearCenters 
                { 
                    Elements = UtilsEnumerable.ArrayOf(bestCandidate.FistNewCurve, bestCandidate.SecondNewCurve, bestCandidate.ExistingCurve)
                };
                return UtilsEnumerable.Singleton(annotation);
            }
            else
                return Enumerable.Empty<Annotation>();
        }

        private bool AreGoodCandidates(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            bool areAlmostColinear = AreAlmostColinear(firstNewCurve, secondNewCurve, existingCurve);
            bool areCloseEnough = AreCloseEnough(firstNewCurve, secondNewCurve, existingCurve);
            bool areAlmostParallel = AreAlmostParallel(firstNewCurve, secondNewCurve, existingCurve);

            return areAlmostColinear && areCloseEnough;
        }

        private bool AreAlmostParallel(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            var firstCrossLength = Vector3D.CrossProduct(firstNewCurve.NormalResult, secondNewCurve.NormalResult).Length;
            var secondCrossLength = Vector3D.CrossProduct(firstNewCurve.NormalResult, existingCurve.NormalResult).Length;
            var thirdCrossLength = Vector3D.CrossProduct(secondNewCurve.NormalResult, existingCurve.NormalResult).Length;

            var maxCross = Math.Max(firstCrossLength, Math.Max(secondCrossLength, thirdCrossLength));
            var angle = Math.Acos(maxCross);

            return Math.Abs(angle) <= parallelismThreshold;
        }

        private bool AreCloseEnough(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            return ProximityMeasure(firstNewCurve, secondNewCurve, existingCurve) <= proximityThreshold;
        }

        private bool AreAlmostColinear(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            double[] angles = { GetAngle(firstNewCurve, secondNewCurve, existingCurve),
                                GetAngle(firstNewCurve, existingCurve, secondNewCurve),
                                GetAngle(secondNewCurve, firstNewCurve, existingCurve) };
            var minAngle = angles.Min();
            return minAngle < colinearityThreshold;
        }

        private double GetAngle(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            var c1 = firstNewCurve.CenterResult;
            var c2 = secondNewCurve.CenterResult;
            var c3 = existingCurve.CenterResult;

            var u = c1 - c2;
            var v = c3 - c1;

            var cos = Vector3D.DotProduct(u, v) / (u.Length * v.Length);
            var angle = Math.Acos(cos);

            return angle;
        }

        private double ProximityMeasure(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            var d1 = (existingCurve.CenterResult - firstNewCurve.CenterResult).Length;
            var d2 = (existingCurve.CenterResult - secondNewCurve.CenterResult).Length;
            return Math.Min(d1, d2);
        }
    }
}
