using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Infrastructure.Services
{
    /// <summary>
    /// Responsible for automatic inference of annotations based on the current state of the model.
    /// </summary>
    public interface IAnnotationInference
    {
        /// <summary>
        /// Attempts to infer annotations for a primitive to be snapped.
        /// </summary>
        /// <param name="toBeSnapped">The new primitive that is going to be snapped.</param>
        /// <param name="toBeAnnotated">The snapped version of the <paramref name="toBeSnapped"/> that is going to 
        /// be annotated.</param>
        /// <returns>A collection of the inferred annotations.</returns>
        IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated);
    }
}
