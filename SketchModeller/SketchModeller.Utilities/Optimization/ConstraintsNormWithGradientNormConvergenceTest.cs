using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    /// <summary>
    /// Tests for convergence based on the norm of the constraints and the norm of the gradient of the lagrangian.
    /// </summary>
    public class ConstraintsNormWithGradientNormConvergenceTest : IConvergenceTest
    {
        private readonly double constraintsNormMax;
        private readonly double lagrangianGradientNormMax;
        private readonly int maxIterations;

        private int iterationsCount;

        /// <summary>
        /// Constructs a new instance of the <see cref="ConstraintsNormWithGradientNormConvergenceTest"/> class.
        /// </summary>
        /// <param name="constraintsNormMax">The maximum value of allowed constraints norm. When the constraints norm goes
        /// below this value the algorithm can converge.</param>
        /// <param name="lagrangianGradientNormMax">The amximum value of allowed norm for the gradient of the Lagrangian. When this
        /// norm goes below this value the algorithm can converge.</param>
        /// <param name="maxIterations">The maximal allowed number of iterations</param>
        public ConstraintsNormWithGradientNormConvergenceTest(
            double constraintsNormMax,
            double lagrangianGradientNormMax,
            int maxIterations)
        {
            this.constraintsNormMax = constraintsNormMax;
            this.lagrangianGradientNormMax = lagrangianGradientNormMax;
            this.maxIterations = maxIterations;
        }

        public void Reset()
        {
            iterationsCount = 0;
            HasConverged = false;
        }

        public void Update(AugmentedLagrangianIterationResult iterationResult)
        {
            ++iterationsCount;

            bool constraintsCondition = iterationResult.ConstraintsNorm <= constraintsNormMax;
            bool gradientNormCondition = iterationResult.LagrangianGradientNorm <= lagrangianGradientNormMax;
            bool iterationsCountCondition = iterationsCount <= maxIterations;
            HasConverged = constraintsCondition && gradientNormCondition && iterationsCountCondition;
        }

        public bool HasConverged { get; set; }
    }
}
