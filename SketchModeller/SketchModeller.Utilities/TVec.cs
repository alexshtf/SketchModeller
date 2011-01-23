using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using Utils;

namespace SketchModeller.Utilities
{
    /// <summary>
    /// A column vector made of terms.
    /// </summary>
    public class TVec
    {
        private readonly Term[] terms;

        public TVec(IEnumerable<Term> terms)
        {
            Contract.Requires(terms != null);
            Contract.Requires(!terms.IsEmpty());

            this.terms = terms.ToArray();
        }

        public TVec(params Term[] terms)
            : this(terms as IEnumerable<Term>)
        {
            Contract.Requires(terms != null);
            Contract.Requires(terms.Length > 0);
        }

        public TVec(TVec first, params Term[] rest)
            : this(first.terms.Concat(rest ?? System.Linq.Enumerable.Empty<Term>()))
        {
            Contract.Requires(first != null);
        }

        private TVec(Term[] left, Term[] right, Func<Term, Term, Term> elemOp)
        {
            Contract.Assume(left.Length == right.Length);
            terms = new Term[left.Length];
            for (int i = 0; i < terms.Length; ++i)
                terms[i] = elemOp(left[i], right[i]);
        }

        private TVec(Term[] input, Func<Term, Term> elemOp)
        {
            terms = new Term[input.Length];
            for (int i = 0; i < input.Length; ++i)
                terms[i] = elemOp(input[i]);
        }

        public Term this[int index]
        {
            get { return terms[index]; }
        }

        public Term NormSquared
        {
            get 
            {
                var powers = terms.Select(x => TermBuilder.Power(x, 2));
                return TermBuilder.Sum(powers);
            }
        }

        public int Dimension
        {
            get { return terms.Length; }
        }

        public Term[] GetTerms()
        {
            return (Term[])terms.Clone();
        }

        public static TVec operator+(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == right.Dimension);
            Contract.Ensures(Contract.Result<TVec>().Dimension == left.Dimension);

            return new TVec(left.terms, right.terms, (x, y) => x + y);
        }

        public static TVec operator-(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == right.Dimension);
            Contract.Ensures(Contract.Result<TVec>().Dimension == left.Dimension);

            return new TVec(left.terms, right.terms, (x, y) => x + y);
        }

        public static TVec operator-(TVec vector)
        {
            return vector * -1;
        }

        public static TVec operator*(TVec vector, Term scalar)
        {
            Contract.Requires(vector != null);
            Contract.Ensures(Contract.Result<TVec>().Dimension == vector.Dimension);

            return new TVec(vector.terms, x => scalar * x);
        }

        public static TVec operator*(Term scalar, TVec vector)
        {
            return vector * scalar;
        }

        public static Term operator*(TVec left, TVec right)
        {
            return InnerProduct(left, right);
        }

        public static Term InnerProduct(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == right.Dimension);
            Contract.Ensures(Contract.Result<Term>() != null);

            var products = from i in System.Linq.Enumerable.Range(0, left.Dimension)
                           select left.terms[i] * right.terms[i];

            return TermBuilder.Sum(products);
        }
    }
}
