using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using Utils;

namespace SketchModeller.Utilities
{
    public static class TermUtils
    {
        public static Term SafeAvg(IEnumerable<Term> terms)
        {
            var count = (double)terms.Count();
            if (count > 1)
                return (1 / count) * SafeSum(terms);
            else
                return SafeSum(terms);
        }

        public static Term SafeSum(IEnumerable<Term> terms)
        {
            Contract.Requires(terms != null);
            Contract.Requires(Contract.ForAll(terms, term => term != null));
            Contract.Ensures(Contract.Result<Term>() != null);

            terms = terms.Where(term => !(term is Zero));

            if (!terms.Any())
                return 0;
            else if (!terms.Skip(1).Any())
                return terms.First();
            else
                return TermBuilder.Sum(terms);
        }

        public static TVec Normal3D(TVec p, TVec q, TVec r)
        {
            Contract.Requires(Contract.ForAll(new TVec[] { p, q, r }, x => x != null));
            Contract.Requires(Contract.ForAll(new TVec[] { p, q, r }, x => x.Dimension == 3));

            return TVec.CrossProduct(q - p, r - p);
        }

        public static Term SoftMin(Term[] terms, double exponent = 6)
        {
            Contract.Requires(terms != null);
            Contract.Requires(terms.Length > 0);
            Contract.Requires(Contract.ForAll(terms, term => term != null));
            Contract.Requires(exponent > 1);
            Contract.Ensures(Contract.Result<Term>() != null);

            var powers = terms.Select(term => TermBuilder.Power(term,-exponent));

            return TermBuilder.Power(TermBuilder.Sum(powers), -1 / exponent);
        }


        public static Term DiffSquared(Term[] left, Term[] right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Length == right.Length);

            var toSum = from i in System.Linq.Enumerable.Range(0, left.Length)
                        select TermBuilder.Power(left[i] - right[i], 2);

            return TermBuilder.Sum(toSum);
        }
    }
}
