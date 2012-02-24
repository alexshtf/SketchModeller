using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;

namespace SketchModeller.Utilities.Optimization
{
    class MultistageConstrainedOptimizer : IConstrainedSolver
    {
        private readonly IConstrainedSolver[] stages;

        public MultistageConstrainedOptimizer(params IConstrainedSolver[] stages)
        {
            this.stages = stages.ToArray();
        }

        public IEnumerable<double[]> Solve(Term objective, IEnumerable<Term> constraints, Variable[] variables, double[] startPoint)
        {
            double[] lastSolution = startPoint;
            foreach (var stage in stages)
            {
                var solutions = stage.Solve(objective, constraints, variables, lastSolution);
                foreach (var solution in solutions)
                {
                    yield return solution;
                    lastSolution = solution;
                }
            }
        }
    }
}
