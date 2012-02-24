using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    public class ConstantPenaltySolver : IConstrainedSolver
    {
        private readonly double penaltyWeight;
        private readonly double gradientNormThreshold;
        private readonly IFirstOrderUnconstrainedOptimizer unconstrainedOptimizer;

        public ConstantPenaltySolver(double penaltyWeight, double gradientNormThreshold, IFirstOrderUnconstrainedOptimizer unconstrainedOptimizer)
        {
            this.penaltyWeight = penaltyWeight;
            this.gradientNormThreshold = gradientNormThreshold;
            this.unconstrainedOptimizer = unconstrainedOptimizer;
        }

        public IEnumerable<double[]> Solve(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint)
        {
            var constraintSquares = constraints.Select(c => TermBuilder.Power(c, 2));
            var penalizedObjective = objective + penaltyWeight * TermUtils.SafeSum(constraintSquares);
            var compiledPenalizedObjective = penalizedObjective.Compile(variables);
            var solution = unconstrainedOptimizer.Solve(x => compiledPenalizedObjective.Differentiate(x), startPoint, gradientNormThreshold);
            yield return solution;
        }
    }
}
