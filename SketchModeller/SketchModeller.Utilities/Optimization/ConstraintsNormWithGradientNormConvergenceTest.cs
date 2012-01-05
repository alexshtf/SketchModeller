using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    class ConstraintsNormWithGradientNormConvergenceTest : IConvergenceTest
    {
        private readonly double constraintsNormMax;
        private readonly double lagrangianGradientNormMax;

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
