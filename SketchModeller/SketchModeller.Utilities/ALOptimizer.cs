using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace SketchModeller.Utilities
{
    public static class ALOptimizer
    {
        public static double[] Minimize(
            Term target,
            Term[] constraints,
            Variable[] vars,
            double[] x,
            Func<Func<double[], Tuple<double, double[]>>, double[], double[]> minimizer,
            double mu = 1,
            double tolerance = 1E-6)
        {
            Contract.Requires(target != null);
            Contract.Requires(x != null);
            Contract.Requires(vars != null && vars.Length == x.Length);
            Contract.Requires(minimizer != null);
            Contract.Requires(mu > 0);
            Contract.Requires(tolerance > 0);

            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == x.Length);

            if (constraints == null)
                constraints = new Term[0];

            // generate a Lagrange multiplier for every constraint
            var multipliers =
                Enumerable.Range(0, constraints.Length)
                .Select(_ => new Variable())
                .ToArray();
            var multipliersValues = new double[multipliers.Length];

            // generate sigma[ u_i * c_i(x) ] where u_i are the multipliers.
            var lagrangeMultiplied = TermUtils.SafeSum(
                from i in Enumerable.Range(0, constraints.Length)
                select multipliers[i] * constraints[i]);

            // generate (0.5 / mu) * sigma [ [c_i(x)]² ]
            var penalty = (0.5 / mu) * TermUtils.SafeSum(
                from constraint in constraints
                select TermBuilder.Power(constraint, 2));

            var augmentedLagrangian = target + lagrangeMultiplied + penalty;

            var allVars = vars.Concat(multipliers).ToArray();
            var compiledAg = augmentedLagrangian.Compile(allVars);
            Func<double[], Tuple<double, double[]>> alGradient = arg =>
            {
                var totalArg = arg.Concat(multipliersValues).ToArray();
                var diff = compiledAg.Differentiate(totalArg);
                var value = diff.Item2;
                var gradient = diff.Item1.Take(vars.Length).ToArray();

                return Tuple.Create(value, gradient);
            };

            var compiledConstraints = constraints.Select(c => c.Compile(vars)).ToArray();
            while (Norm2(alGradient(x).Item2) >= tolerance)
            {
                x = minimizer(alGradient, x);

                // calculate constraint violations
                var violations = new double[constraints.Length];
                for (int i = 0; i < constraints.Length; ++i)
                    violations[i] = compiledConstraints[i].Evaluate(x);

                // lambda <- lambda + c(x) / mu
                for (int i = 0; i < constraints.Length; ++i)
                    multipliersValues[i] = multipliersValues[i] + violations[i] / mu;
            }

            return x;
        }

        private static double Norm2(double[] values)
        {
            return Math.Sqrt(values.Select(x => x * x).Sum());
        }
    }
}
