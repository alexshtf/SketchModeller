using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public static class TermUtils
    {
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

            var toSum = from i in Enumerable.Range(0, left.Length)
                        select TermBuilder.Power(left[i] - right[i], 2);

            return TermBuilder.Sum(toSum);
        }
    }
}
