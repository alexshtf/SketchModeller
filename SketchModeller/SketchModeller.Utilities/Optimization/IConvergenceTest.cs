using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    interface IConvergenceTest
    {
        void Reset();
        void Update(AugmentedLagrangianIterationResult iterationResult);
        bool HasConverged { get; }
    }
}
