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

        /// <summary>
        /// Constructs a new instance of the <see cref="ConstraintsNormWithGradientNormConvergenceTest"/> class.
        /// </summary>
        /// <param name="constraintsNormMax">The maximum value of allowed constraints norm. When the constraints norm goes
        /// below this value the algorithm can converge.</param>
        /// <param name="lagrangianGradientNormMax">The amximum value of allowed norm for the gradient of the Lagrangian. When this
        /// norm goes below this value the algorithm can converge.</param>
        public ConstraintsNormWithGradientNormConvergenceTest(
            double constraintsNormMax,
            double lagrangianGradientNormMax)
        {
            this.constraintsNormMax = constraintsNormMax;
            this.lagrangianGradientNormMax = lagrangianGradientNormMax;
        }

        public void Reset()
        {
            HasConverged = false;
        }

        public void Update(AugmentedLagrangianIterationResult iterationResult)
        {
            bool constraintsCondition = iterationResult.ConstraintsNorm <= constraintsNormMax;
            bool gradientNormCondition = iterationResult.LagrangianGradientNorm <= lagrangianGradientNormMax;
            HasConverged = constraintsCondition && gradientNormCondition;
        }

        public bool HasConverged { get; set; }
    }
}
