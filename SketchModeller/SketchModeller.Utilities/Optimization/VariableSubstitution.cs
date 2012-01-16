using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    public static class VariableSubstitution
    {
        public static Term Substitute(this Term term, Variable[] variables, double[] values)
        {
            var dictionary = Enumerable.Range(0, variables.Length).ToDictionary(i => variables[i], i => values[i]);
            var result = term.Substitute(dictionary);
            return result;
        }

        public static Term Substitute(this Term term, IDictionary<Variable, double> substitutions)
        {
            var visitor = new SubsitutionVisitor(substitutions);
            var result = term.Accept(visitor).Term;
            return result;
        }

        class SubstitutionResult
        {
            public Term Term { get; set; }
        }

        class SubsitutionVisitor : ITermVisitor<SubstitutionResult>
        {
            private readonly IDictionary<Variable, double> substitutions;

            public SubsitutionVisitor(IDictionary<Variable, double> substitutions)
            {
                this.substitutions = substitutions;
            }

            public SubstitutionResult Visit(NaryFunc func)
            {
                var argResults = func.Terms.Select(term => term.Accept(this)).ToArray();
                var values = new double[argResults.Length];

                bool areAllConstants = true;
                for (int i = 0; i < argResults.Length; i++)
                {
                    if (TryGetConstant(argResults[i], out values[i]))
                    {
                        areAllConstants = false;
                        break;
                    }
                }

                if (areAllConstants)
                    return CreateResult(func.Eval(values));
                else
                    return CreateResult(new NaryFunc(func.Eval, func.Diff, argResults.Select(result => result.Term)));
            }

            public SubstitutionResult Visit(BinaryFunc func)
            {
                var leftResult = func.Left.Accept(this);
                var rightResult = func.Right.Accept(this);

                double leftValue, rightValue;
                if (TryGetConstant(leftResult, out leftValue) && TryGetConstant(rightResult, out rightValue))
                    return CreateResult(func.Eval(leftValue, rightValue));
                else
                    return CreateResult(new BinaryFunc(func.Eval, func.Diff, leftResult.Term, rightResult.Term));
            }

            public SubstitutionResult Visit(UnaryFunc func)
            {
                var argResult = func.Argument.Accept(this);

                double argValue;
                if (TryGetConstant(argResult, out argValue))
                    return CreateResult(func.Eval(argValue));
                else
                    return CreateResult(new UnaryFunc(func.Eval, func.Diff, argResult.Term));
            }

            public SubstitutionResult Visit(Exp exp)
            {
                var argResult = exp.Arg.Accept(this);

                double argValue;
                if (TryGetConstant(argResult, out argValue))
                    return CreateResult(Math.Exp(argValue));
                else
                    return CreateResult(TermBuilder.Log(argResult.Term));
            }

            public SubstitutionResult Visit(Log log)
            {
                var argResult = log.Arg.Accept(this);

                double argValue;
                if (TryGetConstant(argResult, out argValue))
                    return CreateResult(Math.Log(argValue));
                else
                    return CreateResult(TermBuilder.Log(argResult.Term));
            }

            public SubstitutionResult Visit(Variable variable)
            {
                double value;
                if (substitutions.TryGetValue(variable, out value))
                    return CreateResult(value);
                else
                    return CreateResult(variable);
            }

            public SubstitutionResult Visit(Sum sum)
            {
                var summmandResults = sum.Terms.Select(x => x.Accept(this)).ToArray();

                var nonConstants = new List<Term>();
                double sumValue = 0;
                foreach (var summandResult in summmandResults)
                {
                    double value;
                    if (TryGetConstant(summandResult, out value))
                        sumValue += value;
                    else
                        nonConstants.Add(summandResult.Term);
                }

                if (nonConstants.Count == 0) // all are constants
                    return CreateResult(sumValue);
                else
                {
                    var newSummands = nonConstants.Concat(Enumerable.Repeat(TermBuilder.Constant(sumValue), 1));
                    return CreateResult(TermBuilder.Sum(newSummands));
                }
            }

            public SubstitutionResult Visit(Product product)
            {
                var leftResult = product.Left.Accept(this);
                var rightResult = product.Right.Accept(this);

                double leftValue, rightValue;
                if (TryGetConstant(leftResult, out leftValue) && TryGetConstant(rightResult, out rightValue))
                    return CreateResult(leftValue * rightValue);
                else
                    return CreateResult(leftResult.Term * rightResult.Term);
            }

            public SubstitutionResult Visit(TermPower power)
            {
                var baseResult = power.Base.Accept(this);
                var expResult = power.Exponent.Accept(this);

                double baseValue, expValue;
                if (TryGetConstant(baseResult, out baseValue) && TryGetConstant(expResult, out expValue))
                    return CreateResult(Math.Pow(baseValue, expValue));
                else
                    return CreateResult(new TermPower(baseResult.Term, expResult.Term));
            }

            public SubstitutionResult Visit(ConstPower power)
            {
                var baseResult = power.Base.Accept(this);
                double value;
                if (TryGetConstant(baseResult, out value))
                    return CreateResult(Math.Pow(value, power.Exponent));
                else
                    return CreateResult(new ConstPower(baseResult.Term, power.Exponent));
            }

            public SubstitutionResult Visit(Zero zero)
            {
                return new SubstitutionResult { Term = zero };
            }

            public SubstitutionResult Visit(Constant constant)
            {
                return new SubstitutionResult { Term = constant };
            }

            #region helper methods

            private static bool TryGetConstant(SubstitutionResult baseResult, out double value)
            {
                value = 0;

                if (baseResult.Term is Zero)
                    return true;

                if (baseResult.Term is Constant)
                {
                    value = ((Constant)baseResult.Term).Value;
                    return true;
                }

                return false;
            }

            private static SubstitutionResult CreateResult(double value)
            {
                return CreateResult(TermBuilder.Constant(value));
            }

            private static SubstitutionResult CreateResult(Term term)
            {
                return new SubstitutionResult { Term = term };
            }

            #endregion
        }
    }
}
