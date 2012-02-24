using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using AutoDiff;
using SketchModeller.Utilities.Optimization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SketchModeller.Modelling.Services.ConstrainedOptimizer
{
    class ConstrainedOptimizerService : IConstrainedOptimizer
    {
        public IEnumerable<double[]> Minimize(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            //DebugSave(objective, constraints, vars, startPoint);
            var solver = CreateSolver();
            return solver.Solve(objective, constraints, vars, startPoint);
        }

        private IConstrainedSolver CreateSolver()
        {
            var penaltySolver = new PenaltySolverFactory().Create(10, 1E-8);
            var augmentedLagrangianSolver = new AugmentedLagrangianSolverFactory().Create();
            var multistageSolver = new MultistageConstrainedOptimizer(penaltySolver, augmentedLagrangianSolver);
            return multistageSolver;
        }

        /*
        private void DebugSave(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            var time = DateTime.Now;
            string fileName = "aaaopt" + time.Ticks + ".opt";
            using (var stream = File.Create(fileName))
            {
                var formatter = new BinaryFormatter();
                var tuple = Tuple.Create(objective, constraints.ToArray(), vars, startPoint);
                formatter.Serialize(stream, tuple);
            }
        }*/
    }
}
