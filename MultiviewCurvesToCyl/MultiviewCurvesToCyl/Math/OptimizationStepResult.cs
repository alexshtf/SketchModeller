using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiviewCurvesToCyl
{
    class OptimizationStepResult
    {
        public double CurrentTarget { get; set; }
        public double PrevTarget { get; set; }
        public double[] CurrentMinimizer { get; set; }
        public double[] PrevMinimizer { get; set; }
    }
}
