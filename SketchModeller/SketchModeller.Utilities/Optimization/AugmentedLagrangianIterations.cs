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

            var multipliers = new double[compiledLagrangian.ConstraintsCount];
            var currentPoint = startPoint;

            var constraintsPenalty = startConstraintsPenalty;
            var maxConstraintsNorm = 1 / Math.Pow(constraintsPenalty, 0.1);
            var maxLagrangianGradientNorm = 1 / constraintsPenalty;

            while (true)
            {
                // we compute a new optimal point estimate
                currentPoint = unconstrainedOptimizer.Solve(
                    objectiveWithGradient: x => compiledLagrangian.LagrangianWithGradient(x, multipliers, constraintsPenalty),
                    initialValue: currentPoint,
                    gradientNormThreshold: maxLagrangianGradientNorm);

                // compute the value of each constraint function. These are the constraint violations
                var constraintValues = compiledLagrangian.EvaluateConstraints(currentPoint);
                var constraintsNorm = Math.Sqrt(constraintValues.Select(x => x * x).Sum());
                if (constraintsNorm < maxConstraintsNorm)
                {
                    // compute the norm of the gradient of the lagrangian
                    var lagrangianGradient = compiledLagrangian.LagrangianWithGradient(currentPoint, multipliers, constraintsPenalty).Item1;
                    var lagrangianGradientNorm = Math.Sqrt(lagrangianGradient.Select(x => x * x).Sum());

                    // we now product iteration result
                    yield return new AugmentedLagrangianIterationResult 
                    { 
                        Values = currentPoint, 
                        ConstraintsNorm = constraintsNorm,
                        LagrangianGradientNorm = lagrangianGradientNorm
                    };

                    // update the current lagrange multipliers estimate according to the algorithm's update formula
                    // lambda <-- lambda + c / mu
                    for (int i = 0; i < multipliers.Length; ++i)
                        multipliers[i] = multipliers[i] + constraintValues[i] * constraintsPenalty;
                    
                    maxConstraintsNorm = maxConstraintsNorm / Math.Pow(constraintsPenalty, 0.9);
                    maxConstraintsNorm = Math.Max(maxConstraintsNorm, maxConstraintsNormLowerBound);

                    maxLagrangianGradientNorm = maxLagrangianGradientNorm / constraintsPenalty;
                    maxLagrangianGradientNorm = Math.Max(maxLagrangianGradientNorm, lagrangianGradientNormLowerBound);
                }
                else
                {
                    constraintsPenalty = 2 * constraintsPenalty; 
                    constraintsPenalty = Math.Min(constraintsPenalty, constraintsPenaltyMax);
                    
                    maxConstraintsNorm = 1 / Math.Pow(constraintsPenalty, 0.1);
                    maxConstraintsNorm = Math.Max(maxConstraintsNorm, maxConstraintsNormLowerBound);

                    maxLagrangianGradientNorm = 1 / constraintsPenalty;
                    maxLagrangianGradientNorm = Math.Max(maxLagrangianGradientNorm, lagrangianGradientNormLowerBound);
                }
            }
        }
    }
}
