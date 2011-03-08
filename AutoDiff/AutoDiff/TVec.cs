﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace AutoDiff
{
    /// <summary>
    /// A column vector made of terms.
    /// </summary>
    [Serializable]
    public class TVec
    {
        private readonly Term[] terms;

        /// <summary>
        /// Constructs a new instance of the <see cref="TVec"/> class given vector components.
        /// </summary>
        /// <param name="terms">The vector component terms</param>
        public TVec(IEnumerable<Term> terms)
        {
            Contract.Requires(terms != null);
            Contract.Requires(Contract.ForAll(terms, term => term != null));
            Contract.Requires(!terms.IsEmptyEnumerable());

            this.terms = terms.ToArray();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TVec"/> class given vector components.
        /// </summary>
        /// <param name="terms">The vector component terms</param>
        public TVec(params Term[] terms)
            : this(terms as IEnumerable<Term>)
        {
            Contract.Requires(terms != null);
            Contract.Requires(Contract.ForAll(terms, term => term != null));
            Contract.Requires(terms.Length > 0);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="TVec"/> class using another vector's components.
        /// </summary>
        /// <param name="first">A vector containing the first vector components to use.</param>
        /// <param name="rest">More vector components to add in addition to the components in <paramref name="first"/></param>
        public TVec(TVec first, params Term[] rest)
            : this(first.terms.Concat(rest ?? System.Linq.Enumerable.Empty<Term>()))
        {
            Contract.Requires(first != null);
            Contract.Requires(Contract.ForAll(rest, term => term != null));
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

        /// <summary>
        /// Gets a vector component given its zero-based index.
        /// </summary>
        /// <param name="index">The vector's component index.</param>
        /// <returns>The vector component.</returns>
        public Term this[int index]
        {
            get 
            {
                Contract.Requires(index >= 0 && index < Dimension);
                Contract.Ensures(Contract.Result<Term>() != null);

                return terms[index]; 
            }
        }

        /// <summary>
        /// Gets a term representing the squared norm of this vector.
        /// </summary>
        public Term NormSquared
        {
            get 
            {
                Contract.Ensures(Contract.Result<Term>() != null);

                var powers = terms.Select(x => TermBuilder.Power(x, 2));
                return TermBuilder.Sum(powers);
            }
        }

        /// <summary>
        /// Gets the dimensions of this vector
        /// </summary>
        public int Dimension
        {
            get 
            {
                Contract.Ensures(Contract.Result<int>() > 0);

                return terms.Length; 
            }
        }

        /// <summary>
        /// Gets the first vector component
        /// </summary>
        public Term X
        {
            get 
            {
                Contract.Ensures(Contract.Result<Term>() != null);

                return this[0]; 
            }
        }

        /// <summary>
        /// Gets the second vector component.
        /// </summary>
        public Term Y
        {
            get 
            { 
                Contract.Requires(Dimension >= 2);
                Contract.Ensures(Contract.Result<Term>() != null);

                return this[1];
            }
        }

        /// <summary>
        /// Gets the third vector component
        /// </summary>
        public Term Z
        {
            get
            {
                Contract.Requires(Dimension >= 3);
                Contract.Ensures(Contract.Result<Term>() != null);

                return this[2];
            }
        }

        /// <summary>
        /// Gets an array of all vector components.
        /// </summary>
        /// <returns>An array of all vector components. Users are free to modify this array. It doesn't point to any
        /// internal structures.</returns>
        public Term[] GetTerms()
        {
            Contract.Ensures(Contract.Result<Term[]>() != null);
            Contract.Ensures(Contract.Result<Term[]>().Length > 0);
            Contract.Ensures(Contract.ForAll(Contract.Result<Term[]>(), term => term != null));

            return (Term[])terms.Clone();
        }

        /// <summary>
        /// Constructs a sum of two term vectors.
        /// </summary>
        /// <param name="left">The first vector in the sum</param>
        /// <param name="right">The second vector in the sum</param>
        /// <returns>A vector representing the sum of <paramref name="left"/> and <paramref name="right"/></returns>
        public static TVec operator+(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == right.Dimension);
            Contract.Ensures(Contract.Result<TVec>().Dimension == left.Dimension);

            return new TVec(left.terms, right.terms, (x, y) => x + y);
        }

        /// <summary>
        /// Constructs a difference of two term vectors,
        /// </summary>
        /// <param name="left">The first vector in the difference</param>
        /// <param name="right">The second vector in the difference.</param>
        /// <returns>A vector representing the difference of <paramref name="left"/> and <paramref name="right"/></returns>
        public static TVec operator-(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == right.Dimension);
            Contract.Ensures(Contract.Result<TVec>().Dimension == left.Dimension);

            return new TVec(left.terms, right.terms, (x, y) => x - y);
        }

        /// <summary>
        /// Inverts a vector
        /// </summary>
        /// <param name="vector">The vector to invert</param>
        /// <returns>A vector repsesenting the inverse of <paramref name="vector"/></returns>
        public static TVec operator-(TVec vector)
        {
            Contract.Requires(vector != null);
            Contract.Ensures(Contract.Result<TVec>().Dimension == vector.Dimension);

            return vector * -1;
        }

        /// <summary>
        /// Multiplies a vector by a scalar
        /// </summary>
        /// <param name="vector">The vector</param>
        /// <param name="scalar">The scalar</param>
        /// <returns>A product of the vector <paramref name="vector"/> and the scalar <paramref name="scalar"/>.</returns>
        public static TVec operator*(TVec vector, Term scalar)
        {
            Contract.Requires(vector != null);
            Contract.Requires(scalar != null);
            Contract.Ensures(Contract.Result<TVec>().Dimension == vector.Dimension);

            return new TVec(vector.terms, x => scalar * x);
        }

        /// <summary>
        /// Multiplies a vector by a scalar
        /// </summary>
        /// <param name="vector">The vector</param>
        /// <param name="scalar">The scalar</param>
        /// <returns>A product of the vector <paramref name="vector"/> and the scalar <paramref name="scalar"/>.</returns>
        public static TVec operator *(Term scalar, TVec vector)
        {
            Contract.Requires(vector != null);
            Contract.Requires(scalar != null);
            Contract.Ensures(Contract.Result<TVec>().Dimension == vector.Dimension);

            return vector * scalar;
        }

        /// <summary>
        /// Constructs a term representing the inner product of two vectors.
        /// </summary>
        /// <param name="left">The first vector of the inner product</param>
        /// <param name="right">The second vector of the inner product</param>
        /// <returns>A term representing the inner product of <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static Term operator*(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == right.Dimension);
            Contract.Ensures(Contract.Result<Term>() != null);

            return InnerProduct(left, right);
        }

        /// <summary>
        /// Constructs a term representing the inner product of two vectors.
        /// </summary>
        /// <param name="left">The first vector of the inner product</param>
        /// <param name="right">The second vector of the inner product</param>
        /// <returns>A term representing the inner product of <paramref name="left"/> and <paramref name="right"/>.</returns>
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

        /// <summary>
        /// Constructs a 3D cross-product vector given two 3D vectors.
        /// </summary>
        /// <param name="left">The left cross-product term</param>
        /// <param name="right">The right cross product term</param>
        /// <returns>A vector representing the cross product of <paramref name="left"/> and <paramref name="right"/></returns>
        public static TVec CrossProduct(TVec left, TVec right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Dimension == 3);
            Contract.Requires(right.Dimension == 3);
            Contract.Ensures(Contract.Result<TVec>().Dimension == 3);

            return new TVec(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X
                );
        }
    }
}
