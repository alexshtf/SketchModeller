using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    /// <summary>
    /// This class is responsible for using multiple <see cref="Inferrer"/> objects to infer specific kinds of annotations,
    /// and concatinate the results of all inferrers. Each inferrer infers a specific kind of relationship (orthogonality, collinear centers,...).
    /// </summary>
    class InferenceEngine
    {
        private readonly List<IInferrer> inferrers;

        /// <summary>
        /// Constructs a new instance of the <see cref="InferenceEngine"/> class.
        /// </summary>
        /// <param name="inferrers">The list of inferrers to use.</param>
        public InferenceEngine(params IInferrer[] inferrers)
        {
            this.inferrers = new List<IInferrer>(inferrers);
        }

        /// <summary>
        /// Concatinates the results of all inferrers for the given input to a single collection of annotations.
        /// </summary>
        /// <param name="toBeSnapped">The new primitive to be snapped</param>
        /// <param name="toBeAnnotated">The snapped version of <see cref="toBeAnnotated"/></param>
        /// <returns>A list of all the inferred annotations for this new primitive</returns>
        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            return inferrers.SelectMany(inferrer => inferrer.InferAnnotations(toBeSnapped, toBeAnnotated));
        }
    }
}
