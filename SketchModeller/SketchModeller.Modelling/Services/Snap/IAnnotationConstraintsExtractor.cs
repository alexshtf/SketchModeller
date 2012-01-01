using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Snap
{
    interface IAnnotationConstraintsExtractor
    {
        IEnumerable<Term> GetConstraints(Annotation annotation);
    }
}
