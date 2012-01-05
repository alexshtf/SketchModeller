using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Utilities.Optimization
{
    public class AugmentedLagrangianIterationResult
    {
        public double[] Values { get; set; }
        public double ConstraintsNorm { get; set; }
        public double LagrangianGradientNorm { get; set; }
    }
}
