using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    /// <summary>
    /// This class is responsible for constructing the inference engine in its constructor, and using it in the InferAnnotations method.
    /// </summary>
    class AnnotationInferenceService : IAnnotationInference
    {
        private readonly InferenceEngine inferenceEngine;

        public AnnotationInferenceService(SessionData sessionData, InferenceOptions inferenceOptions)
        {
            this.inferenceEngine = new InferenceEngine(
                new InferrerEntry { Inferrer = new OrthogonalityInferrer(sessionData), IsEnabledPredicate = () => inferenceOptions.OrthogonalAxes },
                new InferrerEntry { Inferrer = new ColinearCentersInferer(sessionData), IsEnabledPredicate = () => inferenceOptions.CollinearCenters },
                new InferrerEntry { Inferrer = new ParallelismInferer(sessionData), IsEnabledPredicate = () => inferenceOptions.Parallelism },
                new InferrerEntry { Inferrer = new CoplanarityInferer(sessionData), IsEnabledPredicate = () => inferenceOptions.Coplanarity },
                new InferrerEntry { Inferrer = new OnSphereInferrer(sessionData), IsEnabledPredicate = () => inferenceOptions.OnSphere },
                new InferrerEntry { Inferrer = new CocentralityInferrer(sessionData), IsEnabledPredicate = () => inferenceOptions.Cocentrality },
                new InferrerEntry { Inferrer = new SameRadiusInferrer(sessionData), IsEnabledPredicate = () => inferenceOptions.SameRadius });
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            return inferenceEngine.InferAnnotations(toBeSnapped, toBeAnnotated);
        }
    }
}
