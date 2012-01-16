using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using SketchModeller.Infrastructure.Data;
using Utils;
using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Services.Snap
{
    class AnnotationConstraintsExtractor : IAnnotationConstraintsExtractor
    {
        public IEnumerable<Term> GetConstraints(Annotation annotation)
        {
            var constraints = new Term[0];
            annotation.MatchClass<Parallelism>(parallelism => constraints = GetConcreteAnnotationTerm(parallelism));
            annotation.MatchClass<Coplanarity>(coplanarity => constraints = GetConcreteAnnotationTerm(coplanarity));
            annotation.MatchClass<Cocentrality>(cocentrality => constraints = GetConcreteAnnotationTerm(cocentrality));
            annotation.MatchClass<ColinearCenters>(colinearCenters => constraints = GetConcreteAnnotationTerm(colinearCenters));
            annotation.MatchClass<CoplanarCenters>(coplanarCenters => constraints = GetConcreteAnnotationTerm(coplanarCenters));
            annotation.MatchClass<OrthogonalAxis>(orthogonalAxes => constraints = GetConcreteAnnotationTerm(orthogonalAxes));
            annotation.MatchClass<OnSphere>(onSphere => constraints = GetConcreteAnnotationTerm(onSphere));
            annotation.MatchClass<SameRadius>(sameRadius => constraints = GetConcreteAnnotationTerm(sameRadius));
            return constraints;
        }

        private Term[] GetConcreteAnnotationTerm(SameRadius sameRadius)
        {
            var elements = sameRadius.Elements.OfType<CircleFeatureCurve>();
            var result = from pair in elements.SeqPairs()
                         let r1 = pair.Item1.Radius
                         let r2 = pair.Item2.Radius
                         select r1 - r2;
            return result.ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(OnSphere onSphere)
        {
            var center = onSphere.SphereOwned.Center;
            var radius = onSphere.SphereOwned.Radius;

            var touchingPoint = onSphere.CenterTouchesSphere.Center;
            var touchingNormal = onSphere.CenterTouchesSphere.Normal;

            var radiusConstraint = (center - touchingPoint).NormSquared - TermBuilder.Power(radius, 2);
            var normalConstraint = VectorParallelism(center - touchingPoint, touchingNormal);

            return normalConstraint.Append(radiusConstraint).ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(OrthogonalAxis orthoonalAxes)
        {
            if (orthoonalAxes.Elements.Length != 2)
                return Enumerable.Empty<Term>().ToArray();

            var firstNormal = orthoonalAxes.Elements[0].Normal;
            var secondNormal = orthoonalAxes.Elements[1].Normal;
            var innerProduct = firstNormal * secondNormal;

            return new Term[] { innerProduct };
        }

        private Term[] GetConcreteAnnotationTerm(CoplanarCenters coplanarCenters)
        {
            var constraints = new List<Term>();
            if (coplanarCenters.Elements.Length >= 2)
            {
                foreach (var pair in coplanarCenters.Elements.SeqPairs())
                {
                    var c1 = pair.Item1.Center;
                    var n1 = pair.Item1.Normal;
                    var c2 = pair.Item2.Center;
                    var n2 = pair.Item2.Normal;

                    var term = (c2 - c1) * TVec.CrossProduct(n1, n2);
                    constraints.Add(term);
                }
            }

            return constraints.ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(Cocentrality cocentrality)
        {
            var constraints = new List<Term>();
            if (cocentrality.Elements.Length >= 2)
            {
                foreach (var pair in cocentrality.Elements.SeqPairs())
                {
                    var fc1 = pair.Item1;
                    var fc2 = pair.Item2;
                    constraints.Add(fc1.Center.X - fc2.Center.X);
                    constraints.Add(fc1.Center.Y - fc2.Center.Y);
                    constraints.Add(fc1.Center.Z - fc2.Center.Z);
                }
            }

            return constraints.ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(Coplanarity coplanarity)
        {
            var constraints = new List<Term>();

            if (coplanarity.Elements.Length >= 2)
            {
                var terms = new List<Term>();
                foreach (var pair in coplanarity.Elements.SeqPairs())
                {
                    var fst = pair.Item1;
                    var snd = pair.Item2;

                    var p1 = fst.Center;
                    var p2 = snd.Center;

                    var n1 = fst.Normal;
                    var n2 = snd.Normal;

                    constraints.AddRange(VectorParallelism(n1, n2));
                    constraints.AddRange(PointsOnPlaneConstraint(p1, n1, new TVec[] { p2 }));
                    constraints.AddRange(PointsOnPlaneConstraint(p2, n2, new TVec[] { p1 }));
                }
            }
            return constraints.ToArray();
        }

        private Term[] PointsOnPlaneConstraint(TVec p, TVec n, IEnumerable<TVec> pts)
        {
            var constraints =
                from x in pts
                let diff = p - x
                select diff * n;
            return constraints.ToArray();
        }


        private Term[] GetConcreteAnnotationTerm(Parallelism parallelism)
        {
            var terms = new List<Term>();
            if (parallelism.Elements.Length >= 2)
            {
                var normals = from elem in parallelism.Elements
                              select elem.Normal;

                foreach (var normalsPair in normals.SeqPairs())
                {
                    var n1 = normalsPair.Item1;
                    var n2 = normalsPair.Item2;

                    terms.AddRange(VectorParallelism(n1, n2));
                }
            }
            return terms.ToArray();
        }

        private Term[] GetConcreteAnnotationTerm(ColinearCenters colinearCenters)
        {
            var terms = new List<Term>();
            if (colinearCenters.Elements.Length >= 3)
            {
                var centers = from elem in colinearCenters.Elements
                              select elem.Center;

                foreach (var triple in centers.SeqTripples())
                {
                    var u = triple.Item1 - triple.Item2;
                    var v = triple.Item2 - triple.Item3;

                    terms.AddRange(VectorParallelism(u, v));
                }
            }
            return terms.ToArray();
        }

        private static IEnumerable<Term> VectorParallelism(TVec u, TVec v)
        {
            yield return u.X * v.Y - v.X * u.Y;
            yield return u.Y * v.Z - u.Z * v.Y;
            yield return u.X * v.Z - v.X * u.Z;
        }
    }
}
