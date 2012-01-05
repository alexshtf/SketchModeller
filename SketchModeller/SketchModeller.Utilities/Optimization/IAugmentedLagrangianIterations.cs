using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    interface IAugmentedLagrangianIterations
    {
        IEnumerable<AugmentedLagrangianIterationResult> Start(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint);
    }
}
