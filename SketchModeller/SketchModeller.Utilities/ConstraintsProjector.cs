using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data.EditConstraints;
using System.Collections.ObjectModel;
using AutoDiff;
using System.Diagnostics.Contracts;
using SketchModeller.Infrastructure.Services;

namespace SketchModeller.Utilities
{
    public class ConstraintsProjector
    {
        private readonly IConstrainedOptimizer optimizer;
        private readonly ReadOnlyCollection<PrimitiveEditConstraint> constraints;

        public ConstraintsProjector(IConstrainedOptimizer optimizer, IEnumerable<PrimitiveEditConstraint> constraints)
        {
            this.optimizer = optimizer;
            this.constraints = Array.AsReadOnly(constraints.ToArray());
        }

        public void Project()
        {
            if (constraints.Count > 0)
            {
                var optimizationProgram = GetOptimizationProblem(constraints);
                var optimalSolution = SolveProgram(optimizationProgram);
                ApplySolution(optimizationProgram.Variables, optimalSolution);
            }
        }

        #region Project method steps (build problem, solve, apply solution)

        private static void ApplySolution(ParameterVariable[] variables, double[] optimalSolution)
        {
            Contract.Requires(variables != null && optimalSolution != null);
            Contract.Requires(variables.Length == optimalSolution.Length);

            foreach (var i in Enumerable.Range(0, variables.Length))
            {
                var parameter = variables[i].Parameter;
                var values = parameter.GetValues();
                values[variables[i].Index] = optimalSolution[i];
                parameter.SetValues(values);
            }
        }

        private double[] SolveProgram(OptimizationProblem optimizationProgram)
        {
            // construct initial value for optimization to be the current value of the parameters
            var variables = optimizationProgram.Variables;
            var initial = GetCurrentValues(variables);

            // run the optimizer
            var minimizer = optimizer.Minimize(
                optimizationProgram.Objective, 
                optimizationProgram.Constraints, 
                optimizationProgram.Variables, 
                initial);

            return minimizer;
        }

        private static OptimizationProblem GetOptimizationProblem(IEnumerable<PrimitiveEditConstraint> constraints)
        {
            // generate constraint terms
            var constraintTerms = new List<Term>();
            var paramsToVars = new Dictionary<IEditParameter, ParameterVariable[]>();
            foreach (var constraint in constraints)
            {
                TryAxisOnLine(constraintTerms, paramsToVars, constraint);
                TryPointOnPlane(constraintTerms, paramsToVars, constraint);
            }

            // get variables
            var variables =
                (from vars in paramsToVars.Values
                 from var in vars
                 select var).ToArray();

            // construct objective --> sum of squared distances to current values
            var values = GetCurrentValues(variables);
            var objective = TermUtils.SafeSum(
                from i in Enumerable.Range(0, variables.Length)
                select TermBuilder.Power(variables[i] - values[i], 2));

            return new OptimizationProblem
            {
                Constraints = constraintTerms.ToArray(),
                Variables = variables,
                Objective = objective,
            };
        }

        #endregion

        #region specific constraints handling

        private static void TryPointOnPlane(List<Term> constraintTerms, IDictionary<IEditParameter, ParameterVariable[]> paramsToVars, PrimitiveEditConstraint constraint)
        {
            var pointOnPlaneConstraint = constraint as PointOnPlaneConstraint;
            if (pointOnPlaneConstraint != null)
            {
                var pnt = GetVariables(paramsToVars, pointOnPlaneConstraint.Point);
                var planePnt = pointOnPlaneConstraint.PlanePoint.ToTermVector();
                var planeNormal = pointOnPlaneConstraint.PlaneNormal.ToTermVector();
                constraintTerms.Add(PointOnPlaneTerm(pnt, planePnt, planeNormal));
            }
        }

        private static void TryAxisOnLine(List<Term> constraintTerms, IDictionary<IEditParameter, ParameterVariable[]> paramsToVars, PrimitiveEditConstraint constraint)
        {
            var axisOnLineConstraint = constraint as AxisOnLineConstraint;
            if (axisOnLineConstraint != null)
            {
                var axisPnt = GetVariables(paramsToVars, axisOnLineConstraint.AxisPoint);
                var axisDir = GetVariables(paramsToVars, axisOnLineConstraint.AxisDirection);

                var linePnt = axisOnLineConstraint.LinePoint.ToTermVector();
                var lineDir = axisOnLineConstraint.LineDirection.ToTermVector();

                constraintTerms.AddRange(VectorsParallelism3DTerms(axisDir, lineDir));
                constraintTerms.AddRange(PointOnLineTerms(axisPnt, linePnt, lineDir));
            }
        }

        #endregion

        #region Basic geometric terms

        private static Term PointOnPlaneTerm(TVec pnt, TVec planePnt, TVec planeNormal)
        {
            var u = pnt - planePnt;
            return OrthogonalVectors3DTerm(u, planeNormal);
        }

        private static Term OrthogonalVectors3DTerm(TVec u, TVec v)
        {
            return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
        }

        private static IEnumerable<Term> PointOnLineTerms(TVec pnt, TVec linePnt, TVec lineDir)
        {
            var u = pnt - linePnt;
            return VectorsParallelism3DTerms(u, lineDir);
        }

        private static IEnumerable<Term> VectorsParallelism3DTerms(TVec u, TVec v)
        {
            yield return u.X * v.Y - u.Y * v.X;
            yield return u.Y * v.Z - u.Z * v.Y;
        }

        #endregion

        #region Utility methods
        
        private static double[] GetCurrentValues(ParameterVariable[] variables)
        {
            var initial = new double[variables.Length];
            foreach (var i in Enumerable.Range(0, variables.Length))
            {
                var variable = variables[i];
                var values = variable.Parameter.GetValues();
                initial[i] = values[variable.Index];
            }
            return initial;
        }

        private static TVec GetVariables(IDictionary<IEditParameter, ParameterVariable[]> paramsToVars, IEditParameter editParameter)
        {
            ParameterVariable[] vars;
            if (!paramsToVars.TryGetValue(editParameter, out vars))
            {
                vars = new ParameterVariable[editParameter.Dimension];
                for (int i = 0; i < editParameter.Dimension; i++)
                    vars[i] = new ParameterVariable(editParameter, i);
                paramsToVars.Add(editParameter, vars);
            }
            return new TVec(vars);
        }

        #endregion

        #region OptimizationProblem class 

        private class OptimizationProblem
        {
            public Term Objective { get; set; }
            public Term[] Constraints { get; set; }
            public ParameterVariable[] Variables { get; set; }
        }

        #endregion

        #region ParameterVariable class

        private class ParameterVariable : Variable
        {
            private readonly IEditParameter parameter;
            private readonly int index;

            public ParameterVariable(IEditParameter parameter, int index)
            {
                this.parameter = parameter;
                this.index = index;
            }

            public IEditParameter Parameter { get { return parameter; } }

            public int Index { get { return index; } }
        }

        #endregion
    }
}
