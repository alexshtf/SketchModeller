using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class SameRadiusInferrer : IInferrer
    {
        private SessionData sessionData;

        public SameRadiusInferrer(SessionData sessionData)
        {
            this.sessionData = sessionData;
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            var thisCircles = toBeAnnotated.FeatureCurves.OfType<CircleFeatureCurve>();
            var allCircles = sessionData.FeatureCurves.OfType<CircleFeatureCurve>();

            var annotations =
                from curve1 in thisCircles
                from curve2 in allCircles.Except(thisCircles)
                where curve1.SnappedTo != null && curve2.SnappedTo != null && curve1.SnappedTo == curve2.SnappedTo
                select new SameRadius { Elements = new FeatureCurve[] { curve1, curve2 } };

            return annotations.ToArray();
        }
    }
}
