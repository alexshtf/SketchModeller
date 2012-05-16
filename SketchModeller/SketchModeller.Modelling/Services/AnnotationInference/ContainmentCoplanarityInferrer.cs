using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.AnnotationInference
{
    class ContainmentCoplanarityInferrer : IInferrer
    {
        public IEnumerable<Annotation> InferAnnotations(NewPrimitive toBeSnapped, SnappedPrimitive toBeAnnotated)
        {
            return Enumerable.Empty<Annotation>();
        }
    }
}
