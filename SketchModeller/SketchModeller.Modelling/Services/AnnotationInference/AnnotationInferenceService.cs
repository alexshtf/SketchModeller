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

        public AnnotationInferenceService(SessionData sessionData)
        {
            this.inferenceEngine = new InferenceEngine(
                new OrthogonalityInferrer(sessionData),
                new ColinearCentersInferer(sessionData),
                new ParallelismInferer(sessionData),
                new CoplanarityInferer(sessionData),
                new OnSphereInferrer(sessionData));
        }

        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            return inferenceEngine.InferAnnotations(toBeSnapped, toBeAnnotated);
        }
    }
}
