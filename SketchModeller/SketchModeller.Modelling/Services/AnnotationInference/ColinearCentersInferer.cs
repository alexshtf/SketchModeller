using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;
using Utils;

using Enumerable = System.Linq.Enumerable;
using UtilsEnumerable = Utils.Enumerable;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class ColinearCentersInferer : IInferrer
    {
        private SessionData sessionData;

        public ColinearCentersInferer(SessionData sessionData)
        {
            this.sessionData = sessionData;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            return Enumerable.Empty<Annotation>();
            var toBeAnnotatedCurves = toBeAnnotated.FeatureCurves;
            var candidateTriples =
                from i in Enumerable.Range(0, toBeAnnotatedCurves.Length)
                from j in Enumerable.Range(i + 1, toBeAnnotatedCurves.Length - i - 1)
                let firstNewCurve = toBeAnnotatedCurves[i]
                let secondNewCurve = toBeAnnotatedCurves[j]
                let allExistingCurves = sessionData.FeatureCurves.Except(toBeAnnotatedCurves)
                from existingCurve in allExistingCurves
                where AreGoodCandidates(firstNewCurve, secondNewCurve, existingCurve)
                select new { FistNewCurve = firstNewCurve, SecondNewCurve = secondNewCurve, ExistingCurve = existingCurve };

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
            throw new NotImplementedException();
        }

        private double ProximityMeasure(FeatureCurve featureCurve, FeatureCurve featureCurve_2, FeatureCurve featureCurve_3)
        {
            throw new NotImplementedException();
        }
    }
}
