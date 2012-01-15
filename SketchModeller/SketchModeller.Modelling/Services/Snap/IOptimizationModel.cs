using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Modelling.Services.Snap
{
    class OptimizationProblem
    {
        public Term Objective { get; set; }
        public Term[] Constraints { get; set; }
        public double[] InitialValue { get; set; }
        public Variable[] Variables { get; set; }
    }

    interface IOptimizationModel
    {
        OptimizationProblem CreateProblem();
        void UpdateSolution(double[] solution);
    }
}
