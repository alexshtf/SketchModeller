using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchModeller.Modelling.Services.Snap
{
    class NoSnappingOptimizationModel : IOptimizationModel
    {
        private readonly IOptimizationModel innerOptimizationModel;

        public NoSnappingOptimizationModel(IOptimizationModel innerOptimizationModel)
        {
            this.innerOptimizationModel = innerOptimizationModel;
        }

        public OptimizationProblem CreateProblem()
        {
            var innerProblem = innerOptimizationModel.CreateProblem();
            innerProblem.Objective = 1;
            return innerProblem;
        }

        public void UpdateSolution(double[] solution)
        {
            innerOptimizationModel.UpdateSolution(solution);
        }
    }
}
