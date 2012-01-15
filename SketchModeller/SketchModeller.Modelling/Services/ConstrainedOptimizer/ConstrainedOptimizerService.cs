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
        private readonly Func<IConstrainedSolver> solverFactory;

        public ConstrainedOptimizerService()
        {
            solverFactory = new AugmentedLagrangianSolverFactory().Create;
        }

        public IEnumerable<double[]> Minimize(Term objective, IEnumerable<Term> constraints, Variable[] vars, double[] startPoint)
        {
            //DebugSave(objective, constraints, vars, startPoint);
            var solver = solverFactory();
            return solver.Solve(objective, constraints, vars, startPoint);
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
