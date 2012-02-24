using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Utilities.Optimization;

namespace SketchModeller.Modelling.Services.ConstrainedOptimizer
{
    class PenaltySolverFactory
    {
        public IConstrainedSolver Create(double penaltyWeight, double gradientNormThreshold)
        {
            var lbfgsSolver = new LBFGSOptimizer(30);
            var penaltySolver = new ConstantPenaltySolver(penaltyWeight, 
                                                          gradientNormThreshold, 
                                                          lbfgsSolver);
            return penaltySolver;
        }
    }
}
