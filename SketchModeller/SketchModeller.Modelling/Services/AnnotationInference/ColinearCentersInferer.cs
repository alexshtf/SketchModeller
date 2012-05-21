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
using System.Diagnostics;
using System.Windows;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class ColinearCentersInferer : IInferrer
    {
        private const double DEFAULT_PROXIMITY_THRESHOLD = 0.6;
        private const double DEFAULT_COLINEARITY_THRESHOLD = 175 * Math.PI / 180; // 10 degrees angle to be considered "colinear"
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
            return areAlmostColinear;
        }

        private bool AreCloseEnough(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            double proximity = ProximityMeasure(firstNewCurve, secondNewCurve, existingCurve);
            return proximity <= proximityThreshold;
        }

        private bool AreAlmostColinear(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            double[] angles = { GetAngle(firstNewCurve, secondNewCurve, existingCurve),
                                GetAngle(firstNewCurve, existingCurve, secondNewCurve),
                                GetAngle(secondNewCurve, firstNewCurve, existingCurve) };
            var maxAngle = angles.Max();

            return maxAngle >= colinearityThreshold;
        }

        private double GetAngle(FeatureCurve firstNewCurve, FeatureCurve secondNewCurve, FeatureCurve existingCurve)
        {
            var c1 = new Point(firstNewCurve.CenterResult.X, -firstNewCurve.CenterResult.Y);
            var c2 = new Point(secondNewCurve.CenterResult.X, -secondNewCurve.CenterResult.Y);
            var c3 = new Point(existingCurve.CenterResult.X, -existingCurve.CenterResult.Y);

            var u = c1 - c2;
            var v = c3 - c1;

            var cos = (u * v) / (u.Length * v.Length);
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
