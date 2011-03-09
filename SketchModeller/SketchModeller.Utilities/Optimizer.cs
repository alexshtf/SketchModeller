using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Utils;
using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Utilities
{
    public static class Optimizer
    {
        public static double[] MinAugmentedLagrangian(
            Term target, 
            Term[] constraints, 
            Variable[] vars, 
            double[] x = null, 
            double mu = 1,
            double tolerance = 1E-6,
            Func<Term, Variable[], double[], double[]> minimizer = null)
        {
            Contract.Requires(target != null);
            Contract.Requires(constraints != null);
            Contract.Requires(Contract.ForAll(constraints, c => c != null));
            Contract.Requires(vars != null);
            Contract.Requires(Contract.ForAll(vars, v => v != null));
            Contract.Requires(x == null || x.Length == vars.Length);
            Contract.Requires(mu > 0);
            Contract.Requires(tolerance > 0);
            Contract.Ensures(Contract.Result<double[]>() != null);
            Contract.Ensures(Contract.Result<double[]>().Length == vars.Length);

            if (x == null)
                x = new double[vars.Length];

            if (minimizer == null)
                minimizer = MinimizeBFGS;

            // lagrange multipliers. initialized to zero.
            double[] lambda = new double[constraints.Length];

            // penalty is sigma of [c_i(x)]²
            var penalty = (0.5 / mu) * TermUtils.SafeSum(constraints.Select(c => TermBuilder.Power(c, 2)));
            var augmentedTarget = target + penalty;

            // a function we will use to constuct the augmented lagrangian A(x; lambda, mu).
            Func<Term> augmentedLagrangian = () =>
                {
                    var lagrangeTerms =
                        from i in Enumerable.Range(0, constraints.Length)
                        select lambda[i] * constraints[i];
                    return augmentedTarget + TermUtils.SafeSum(lagrangeTerms);
                };

            // perform augmented lagrangian iterations.
            Term currentLagrangian = augmentedLagrangian();
            var lagrangianGrad = Differentiator.Differentiate(currentLagrangian, vars, x);
            while (Norm2(lagrangianGrad) >= tolerance)
            {
                // x <- argmin A(x; lambda, mu);
                x = minimizer(currentLagrangian, vars, x);

                // calculate constraint violations
                var violations = new double[constraints.Length];
                for (int i = 0; i < constraints.Length; ++i)
                    violations[i] = Evaluator.Evaluate(constraints[i], vars, x);

                // lambda <- lambda + c(x) / mu
                for (int i = 0; i < constraints.Length; ++i)
                    lambda[i] = lambda[i] + violations[i] / mu;

                // update the current Lagrangian using the new lambdas
                currentLagrangian = augmentedLagrangian();
                lagrangianGrad = Differentiator.Differentiate(currentLagrangian, vars, x);
            }

            return x;
        }

        public static double[] MinimizeBFGS(Term targetFunc, Variable[] vars, double[] startVector = null)
        {
            Contract.Requires(startVector == null || startVector.Length == vars.Length);

            double[] x = startVector == null ? new double[vars.Length] : (double[])startVector.Clone();
            alglib.minlbfgsstate state;
            alglib.minlbfgscreate(1, x, out state);

            var gradProvider = new BFGSProvider(targetFunc, vars);
            alglib.minlbfgsoptimize(state, gradProvider.Grad, null, null);

            alglib.minlbfgsreport rep;
            alglib.minlbfgsresults(state, out x, out rep);

            Trace.WriteLine("Termination type is " + TranslateTerminationType(rep.terminationtype));
            Trace.WriteLine("Number of function evals is " + rep.nfev);
            Trace.WriteLine("Iterations count is " + rep.iterationscount);

            if (rep.terminationtype > 0) // good. no errors
                return x;
            else
                throw new InvalidOperationException("Iteration did not converge!");
        }

        #region GetLMFuncs implementation

        public static Term[] GetLMFuncs(Term targetFunc)
        {
            var resultsWithConstants = RecursiveGetLMFuncs(targetFunc);
            var results =
                from item in resultsWithConstants
                let term = item.Item1
                let factor = Math.Sqrt(item.Item2)
                select factor == 1 ? term : factor * term;
            return results.ToArray();
        }

        private static IEnumerable<Tuple<Term, double>> RecursiveGetLMFuncs(Term term)
        {
            IEnumerable<Tuple<Term, double>> result = null;
            term.MatchClass<Constant>(constant =>
                {
                    var value = constant.Value;
                    result = Utils.Enumerable.Singleton(Tuple.Create((Term)Math.Sqrt(value), 1.0));
                });
            term.MatchClass<Zero>(zero =>
                {
                    result = Utils.Enumerable.Singleton(Tuple.Create((Term)zero, 1.0));
                });
            term.MatchClass<Product>(product =>
                {
                    double constant;
                    Term other;
                    if (GetConstant(product.Left, out constant))
                        other = product.Right;
                    else if (GetConstant(product.Right, out constant))
                        other = product.Left;
                    else
                        throw new InvalidOperationException("The term is not a valid sum-of-squares term");

                    result =
                        from item in RecursiveGetLMFuncs(other)
                        let itemTerm = item.Item1
                        let factor = item.Item2
                        select Tuple.Create(itemTerm, factor * constant);
                });
            term.MatchClass<Sum>(sum =>
                {
                    result =
                        from sumChild in sum.Terms
                        from item in RecursiveGetLMFuncs(sumChild)
                        select item;
                });
            term.MatchClass<IntPower>(power =>
                {
                    if (power.Exponent == 2)
                        result = Utils.Enumerable.Singleton(Tuple.Create(power.Base, 1.0));
                    else
                        throw new InvalidOperationException("The term is not a valid sum-of-squares");
                });
            if (result == null)
                return Utils.Enumerable.Singleton(Tuple.Create(term, 1.0));
            else
                return result;
        }

        private static bool GetConstant(Term term, out double constant)
        {
            constant = 0;
            if (term is Constant)
            {
                constant = ((Constant)term).Value;
                return true;
            }
            if (term is Zero)
                return true;

            return false;
        }

        #endregion

        public static double[] MinimizeLM(Term[] targetFuncs, Variable[] vars, double[] startVector = null, double diffStep = 0.01)
        {
            Contract.Requires(startVector == null || startVector.Length == vars.Length);
            Contract.Requires(targetFuncs != null);
            Contract.Requires(Contract.ForAll(targetFuncs, f => f != null));

            double[] x = startVector == null ? new double[vars.Length] : (double[])startVector.Clone();
            alglib.minlmstate state;
            alglib.minlmcreatevj(targetFuncs.Length, x, out state);

            var lmProvider = new LMProvider(targetFuncs, vars);
            alglib.minlmoptimize(
                state, 
                lmProvider.Eval, 
                lmProvider.Jacobian, 
                (arg, f, obj) => Trace.WriteLine("LM Optimizing. Value is " + f), 
                null);

            alglib.minlmreport rep;
            alglib.minlmresults(state, out x, out rep);

            Trace.WriteLine("Termination type is " + TranslateTerminationType(rep.terminationtype));
            Trace.WriteLine("Iterations count is " + rep.iterationscount);
            Trace.WriteLine("Jacobian evaluations " + rep.njac);
            Trace.WriteLine("Gradient evaluations " + rep.ngrad);
            Trace.WriteLine("Hessian evaluations " + rep.nhess);
            Trace.WriteLine("Cholesky factorizations " + rep.ncholesky);

            if (rep.terminationtype > 0) // good. no errors
                return x;
            else
                throw new InvalidOperationException("Iteration did not converge!");
        }

        private static string TranslateTerminationType(int terminationtype)
        {
            switch (terminationtype)
            {
                case -2:
                    return "rounding errors prevent further improvement.";
                case -1:
                    return "incorrect parameters were specified";
                case 1:
                    return "relative function improvement is no more than EpsF";
                case 2:
                    return "relative step is no more than EpsX";
                case 4:
                    return "gradient norm is no more than EpsG";
                case 5:
                    return "MaxIts steps was taken";
                case 7:
                    return "stopping conditions are too stringent, further improvement is impossible";
                default:
                    return "unknown";
            }
        }

        private static double Norm2(double[] values)
        {
            return Math.Sqrt(values.Select(x => x * x).Sum());
        }

        private class LMProvider
        {
            private readonly Term[] targetFuncs;
            private readonly Variable[] vars;

            public LMProvider(Term[] targetFuncs, Variable[] vars)
            {
                this.targetFuncs = targetFuncs;
                this.vars = vars;
            }

            public void Eval(double[] arg, double[] fi, object obj)
            {
                Contract.Assert(arg.Length == vars.Length);
                Contract.Assert(fi.Length == targetFuncs.Length);

                foreach(var i in Enumerable.Range(0, fi.Length))
                    fi[i] = Evaluator.Evaluate(targetFuncs[i], vars, arg);
            }

            public void Jacobian(double[] arg, double[] fi, double[,] jac, object obj)
            {
                Contract.Assert(arg.Length == vars.Length);
                Contract.Assert(fi.Length == targetFuncs.Length);

                Contract.Assert(jac.GetLength(0) == fi.Length);
                Contract.Assert(jac.GetLength(1) == arg.Length);

                Eval(arg, fi, obj);
                foreach (var i in Enumerable.Range(0, jac.GetLength(0)))
                {
                    var grad = Differentiator.Differentiate(targetFuncs[i], vars, arg);
                    foreach (var j in Enumerable.Range(0, jac.GetLength(1)))
                        jac[i, j] = grad[j];
                }
            }

        }

        private class BFGSProvider
        {
            private readonly Term targetFunc;
            private readonly Variable[] vars;

            public BFGSProvider(Term targetFunc, Variable[] vars)
            {
                this.targetFunc = targetFunc;
                this.vars = vars;
            }

            public void Grad(double[] arg, ref double func, double[] grad, object obj)
            {
                func = Evaluator.Evaluate(targetFunc, vars, arg);
                var tempGrad = Differentiator.Differentiate(targetFunc, vars, arg);
                Contract.Assume(tempGrad.Length == grad.Length);
                Array.Copy(tempGrad, grad, grad.Length);
            }
        }
    }
}
