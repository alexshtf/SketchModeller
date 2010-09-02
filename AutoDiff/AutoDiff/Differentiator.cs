using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Diagnostics.Contracts;

namespace AutoDiff
{
    public static class Differentiator
    {
        public static double[] Differentiate(Term term, Variable[] variables, double[] values)
        {
            Contract.Requires(term != null);
            Contract.Requires(variables != null);
            Contract.Requires(values != null);
            Contract.Requires(variables.Length == values.Length);
            Contract.Ensures(Contract.Result<double[]>().Length == variables.Length);

            return Differentiate(term, variables.Zip(values).ToList());
        }

        public static double[] Differentiate(Term term, IList<Tuple<Variable, double>> values)
        {
            Contract.Requires(term != null);
            Contract.Requires(values != null);
            Contract.Ensures(Contract.Result<double[]>().Length == values.Count);

            var variables = values.Select(x => x.Item1).ToList();
            var valuesDictionary = values.ToDictionary(pair => pair.Item1, pair => pair.Item2);
            var visitor = new DiffVisitor(variables, valuesDictionary);
            term.Accept(visitor);
            return visitor.Gradient.ToArray(values.Count);
        }

        private class DiffVisitor : ITermVisitor
        {
            private readonly IList<Variable> variables;
            private readonly IDictionary<Variable, double> values;
            private readonly IDictionary<Variable, int> indexOf;

            public DiffVisitor(IList<Variable> variables, IDictionary<Variable, double> values)
            {
                this.variables = variables;
                this.values = values;
                indexOf = variables.ZipIndex().ToDictionary(pair => pair.Value, pair => pair.Index);
            }

            public SparseVector Gradient { get; private set; }

            public void Visit(Constant constant)
            {
                Gradient = new SparseVector();
            }

            public void Visit(Zero zero)
            {
                Gradient = new SparseVector();
            }

            public void Visit(IntPower intPower)
            {
                var baseValue = Evaluate(intPower.Base);
                var baseGradient = Differentiate(intPower.Base);

                // n * (f(x1,...,xn))^(n-1) * Df(x1,...,xn)
                Gradient = Scale(baseGradient, intPower.Exponent * Math.Pow(baseValue, intPower.Exponent - 1));
            }

            public void Visit(Product product)
            {
                var leftGrad = Differentiate(product.Left);
                var rightGrad = Differentiate(product.Right);
                var leftVal = Evaluate(product.Left);
                var rightVal = Evaluate(product.Right);

                // Df(x1,..,xn) * g(x1,...,xn) + Dg(x1,...,xn) * f(x1,...,xn)
                Gradient = Add(Scale(leftGrad, rightVal), Scale(rightGrad, leftVal));
            }

            public void Visit(Sum sum)
            {
                var result = Differentiate(sum.Terms[0]);
                foreach (var term in sum.Terms.Skip(1))
                {
                    var grad = Differentiate(term);
                    result = Add(result, grad);
                }

                Gradient = result;
            }

            public void Visit(Variable variable)
            {
                Gradient = new SparseVector(indexOf[variable], 1); // a vector with a single component as 1, and the rest zero
            }

            private double Evaluate(Term t)
            {
                return Evaluator.Evaluate(t, values);
            }

            private SparseVector Differentiate(Term term)
            {
                term.Accept(this);
                return Gradient;
            }

            private static SparseVector Add(SparseVector v1, SparseVector v2)
            {
                return SparseVector.Sum(v1, v2);
            }

            private static SparseVector Scale(SparseVector vector, double scale)
            {
                return SparseVector.Scale(vector, scale);
            }


            public void Visit(Log log)
            {
                var argGrad = Differentiate(log.Arg);
                var argVal = Evaluate(log.Arg);
                if (argVal > 0)
                    Gradient = Scale(argGrad, 1 / argVal);
                else
                    throw new InvalidOperationException("Logarithm has non-positive argument");
            }

            public void Visit(Exp exp)
            {
                var argGrad = Differentiate(exp.Arg);
                var val = Evaluate(exp);
                Gradient = Scale(argGrad, val);
            }
        }

    }
}
