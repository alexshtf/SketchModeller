using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.Snap
{
    interface IPrimitiveReconstructor
    {
        SnappedPrimitive Create(NewPrimitive newPrimitive);

        Tuple<Term, Term[]> Reconstruct(SnappedPrimitive snappedPrimitive,
                                        Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations);
    }
}
