using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    public class LagrangianCompiler : ILagrangianCompiler
    {
        public ILagrangianCompilerResult Compile(Term objective, IEnumerable<Term> constraints, Variable[] variables)
        {
            var multipliers = constraints.Select(_ => new Variable()).ToArray();
            var mu = new Variable();
            var augmentedLagrangian = ConstructAugmentedLagrangian(objective, constraints, multipliers, mu);

            var allParameters = Enumerable.Repeat(mu, 1).Concat(multipliers).ToArray();
            var compiledAugmentedLagrangian = augmentedLagrangian.Compile(variables, allParameters);
            var compiledConstraints = constraints.Select(constraint => constraint.Compile(variables)).ToArray();

            return new Result(compiledAugmentedLagrangian, compiledConstraints);
        }

        private static Term ConstructAugmentedLagrangian(Term objective, IEnumerable<Term> constraints, Variable[] multipliers, Variable mu)
        {
            var constraintsVec = new TVec(constraints);
            var multipliersVec = new TVec(multipliers);
            var augmentedLagrangian = objective + TVec.InnerProduct(multipliersVec, constraintsVec) + mu * constraintsVec.NormSquared;
            return augmentedLagrangian;
        }

        private class Result : ILagrangianCompilerResult
        {
            private readonly IParametricCompiledTerm lagrangian;
            private readonly ICompiledTerm[] constraints;

            public Result(IParametricCompiledTerm lagrangian, ICompiledTerm[] constraints)
            {
                this.lagrangian = lagrangian;
                this.constraints = constraints;
            }

            public Tuple<double[], double> LagrangianWithGradient(double[] arg, double[] multipliers, double mu)
            {
                var parameters = Enumerable.Repeat(mu, 1).Concat(multipliers).ToArray();
                var result = lagrangian.Differentiate(arg, parameters);
                return result;
            }

            public double[] EvaluateConstraints(double[] arg)
            {
                var result = new double[constraints.Length];
                for (int i = 0; i < constraints.Length; i++)
                    result[i] = constraints[i].Evaluate(arg);
                return result;
            }

            public int ConstraintsCount
            {
                get { return constraints.Length; }
            }
        }

    }
}
