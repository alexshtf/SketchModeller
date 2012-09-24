using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics;
using System.Threading;

namespace SketchModeller.Utilities.Optimization
{
    public class AugmentedLagrangianIterations : IAugmentedLagrangianIterations
    {
        private readonly IFirstOrderUnconstrainedOptimizer unconstrainedOptimizer;
        private readonly ILagrangianCompiler lagrangianCompiler;
        private readonly double startConstraintsPenalty;
        private readonly double constraintsPenaltyMax;
        private readonly double maxConstraintsNormLowerBound;
        private readonly double lagrangianGradientNormLowerBound;

        public AugmentedLagrangianIterations(
            IFirstOrderUnconstrainedOptimizer unconstrainedOptimizer, 
            ILagrangianCompiler lagrangianCompiler,
            double startConstraintsPenalty,
            double constraintsPenaltyMax,
            double maxConstraintsNormLowerBound,
            double lagrangianGradientNormLowerBound)
        {
            this.unconstrainedOptimizer = unconstrainedOptimizer;
            this.lagrangianCompiler = lagrangianCompiler;
            this.startConstraintsPenalty = startConstraintsPenalty;
            this.constraintsPenaltyMax = constraintsPenaltyMax;
            this.maxConstraintsNormLowerBound = maxConstraintsNormLowerBound;
            this.lagrangianGradientNormLowerBound = lagrangianGradientNormLowerBound;
        }

        public IEnumerable<AugmentedLagrangianIterationResult> Start(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint)
        {
            var compiledLagrangian = lagrangianCompiler.Compile(objective, constraints, variables);

            const double CONSTRAINT_NORM_EXPONENT_INIT = 0.5;
            const double CONSTRAINT_NORM_EXPONENT_REDUCE = 0.5;

            const double GRADIENT_NORM_EXPONENT_INIT = 0.5;
            const double GRADIENT_NORM_EXPONENT_REDUCE = 0.5;

            var multipliers = new double[compiledLagrangian.ConstraintsCount];
            var currentPoint = startPoint;

            var constraintsPenalty = startConstraintsPenalty;
            var maxConstraintsNorm = 1 / Math.Pow(constraintsPenalty, CONSTRAINT_NORM_EXPONENT_INIT);
            var maxLagrangianGradientNorm = 1 / Math.Pow(constraintsPenalty, GRADIENT_NORM_EXPONENT_INIT);

            while (true)
            {
                // we compute a new optimal point estimate
                currentPoint = unconstrainedOptimizer.Solve(
                    objectiveWithGradient: x => compiledLagrangian.LagrangianWithGradient(x, multipliers, constraintsPenalty), // f(x) + 0.5 * mu * ||c(x)||² + lambda * c(x)
                    initialValue: currentPoint,
                    gradientNormThreshold: maxLagrangianGradientNorm);

                // compute the value of each constraint function. These are the constraint violations
                var constraintValues = compiledLagrangian.EvaluateConstraints(currentPoint);
                var constraintsNorm = Math.Sqrt(constraintValues.Select(x => x * x).Sum());
                if (constraintsNorm < maxConstraintsNorm)
                {
                    // compute the norm of the gradient of the lagrangian
                    var lagrangianGradient = compiledLagrangian.LagrangianWithGradient(currentPoint, multipliers, constraintsPenalty).Item1;
                    var lagrangianGradientNorm = Math.Sqrt(lagrangianGradient.Sum(x => x * x));

                    // we now product iteration result
                    yield return new AugmentedLagrangianIterationResult 
                    { 
                        Values = currentPoint, 
                        ConstraintsNorm = constraintsNorm,
                        LagrangianGradientNorm = lagrangianGradientNorm
                    };

                    // update the current lagrange multipliers estimate according to the algorithm's update formula
                    // lambda <-- lambda + mu * c 
                    for (int i = 0; i < multipliers.Length; ++i)
                        multipliers[i] = multipliers[i] + constraintValues[i] * constraintsPenalty;

                    maxConstraintsNorm = maxConstraintsNorm / Math.Pow(constraintsPenalty, CONSTRAINT_NORM_EXPONENT_REDUCE);
                    maxConstraintsNorm = Math.Max(maxConstraintsNorm, maxConstraintsNormLowerBound);

                    maxLagrangianGradientNorm = maxLagrangianGradientNorm / Math.Pow(constraintsPenalty, GRADIENT_NORM_EXPONENT_REDUCE);
                    maxLagrangianGradientNorm = Math.Max(maxLagrangianGradientNorm, lagrangianGradientNormLowerBound);
                }
                else
                {
                    constraintsPenalty = 2 * constraintsPenalty; 
                    constraintsPenalty = Math.Min(constraintsPenalty, constraintsPenaltyMax);

                    maxConstraintsNorm = 1 / Math.Pow(constraintsPenalty, CONSTRAINT_NORM_EXPONENT_INIT);
                    maxConstraintsNorm = Math.Max(maxConstraintsNorm, maxConstraintsNormLowerBound);

                    maxLagrangianGradientNorm = 1 / Math.Pow(constraintsPenalty, GRADIENT_NORM_EXPONENT_INIT);
                    maxLagrangianGradientNorm = Math.Max(maxLagrangianGradientNorm, lagrangianGradientNormLowerBound);
                }
            }
        }
    }
}
