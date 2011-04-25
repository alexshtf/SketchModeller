using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using AutoDiff;
using Utils;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Services.Snap
{
    class SphereSnapper : BasePrimitivesSnapper<NewSphere, SnappedSphere>
    {
        public static readonly Term[] NO_TERMS = new Term[0];

        protected override SnappedSphere Create(PointsSequence[] selectedCurves, NewSphere newPrimitive)
        {
            var result = new SnappedSphere();
            result.RadiusResult = newPrimitive.Radius;
            result.CenterResult = newPrimitive.Center;

            result.Radius = new Variable();
            result.Center = new TVec(ArrayUtils.Generate<Variable>(3));

            result.ProjectionParallelCircle.SnappedTo = newPrimitive.SilhouetteCircle.AssignedTo;

            return result;
        }

        protected override Tuple<Term, Term[]> Reconstruct(SnappedSphere snappedPrimitive, Dictionary<FeatureCurve, ISet<Annotation>> curvesToAnnotations)
        {
            var circle = CircleFitter.Fit(snappedPrimitive.ProjectionParallelCircle.SnappedTo.Points);

            var result =
                TermBuilder.Power(snappedPrimitive.Center.X - circle.Center.X, 2) +
                TermBuilder.Power(snappedPrimitive.Center.Y + circle.Center.Y, 2) +
                TermBuilder.Power(snappedPrimitive.Radius - circle.Radius, 2);

            return Tuple.Create(result, NO_TERMS);
        }
    }
}
