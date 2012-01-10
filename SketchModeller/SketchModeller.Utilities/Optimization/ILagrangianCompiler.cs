using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// Represents the compiled form of the functions needed for the augmented lagrangian optimizer
    /// </summary>
    public interface ILagrangianCompilerResult
    {
        /// <summary>
        /// Evaluates the value and the gradient of the augmented lagrangian function.
        /// </summary>
        /// <param name="arg">The current optimal point estimate (x)</param>
        /// <param name="multipliers">The current lagrange multipliers estimates (lambda)</param>
        /// <param name="mu">The current constraints penalty parameter</param>
        /// <returns>A tuple with the gradient and the value of the augmented lagrangian function</returns>
        Tuple<double[], double> LagrangianWithGradient(double[] arg, double[] multipliers, double mu);

        /// <summary>
        /// Evaluates the current value of the constraints.
        /// </summary>
        /// <param name="arg">The current optimal point estimate (x)</param>
        /// <returns>An array with the values of the constraint functions</returns>
        double[] EvaluateConstraints(double[] arg);

        /// <summary>
        /// Gets the number of constraints
        /// </summary>
        int ConstraintsCount { get; }
    }

    /// <summary>
    /// Compiles an augmented lagrangian function into an efficient implementation
    /// </summary>
    public interface ILagrangianCompiler
    {
        /// <summary>
        /// Performs compilation of an Augmented-Lagrangian function into an efficient implementation
        /// </summary>
        /// <param name="objective">Objective function term</param>
        /// <param name="constraints">The constraint terms.</param>
        /// <param name="variables">The variables used in the objective and the constraints.</param>
        /// <returns>An interface used to invoke the efficiently-compiled functions.</returns>
        ILagrangianCompilerResult Compile(Term objective, IEnumerable<Term> constraints, Variable[] variables);
    }
}
