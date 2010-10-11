using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Diagnostics.Contracts;
using Utils;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    class FibermeshInflation
    {
        private readonly ConstrainedMesh mesh;
        private readonly MeshTopologyInfo topologyInfo;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(mesh != null);
            Contract.Invariant(topologyInfo != null);
        }

        public FibermeshInflation(ConstrainedMesh mesh)
        {
            Contract.Requires(mesh != null);

            this.mesh = mesh;
            this.topologyInfo = new MeshTopologyInfo(mesh.TriangleIndices);
        }

        public void SmoothStep(List<Tuple<int, Point3D>> constrainedIndices)
        {
            var allIndices = System.Linq.Enumerable.Range(0, mesh.Positions.Count).ToArray();
            var constrainedIndicesArray = constrainedIndices.Select(x => x.Item1).ToArray();
            var constrainedPositionsArray = constrainedIndices.Select(x => x.Item2).ToArray();
            var freeIndicesArray =
                allIndices
                .Except(new HashSet<int>(constrainedIndicesArray))
                .ToArray();

            var scalarVariables = ArrayUtils.Generate<Variable>(mesh.Positions.Count);
            var laplacianTerm = SumSqr(GetLaplacians(scalarVariables, allIndices));


            const double CURVATURE_EQUALITY_WEIGHT = 100;
            var currentCurvatures = GetCurvatureEstimates();
            var curvaturesEqualityTerm = SquareDiff(scalarVariables, ValuesToTerms(currentCurvatures));
            var targetCurvaturesFunction = laplacianTerm + CURVATURE_EQUALITY_WEIGHT * curvaturesEqualityTerm;
            var targetCurvatures = QuadraticMinimize(targetCurvaturesFunction, scalarVariables);


            const double EDGE_LENGTH_EQUALITY_WEIGHT = 100;
            var currentEdgeLengths =
                (from index in allIndices
                 let currentPosition = mesh.Positions[index]
                 let edgeLengths = from neighborIndex in topologyInfo.VertexNeighborsOfVertex(index)
                                   let neighborPosition = mesh.Positions[neighborIndex]
                                   select (currentPosition - neighborPosition).Length
                 select edgeLengths.Average()
                ).ToArray();
            var edgeLengthsEqualityTerm = SquareDiff(scalarVariables, ValuesToTerms(currentEdgeLengths));
            var targetEdgeLengthsFunction = laplacianTerm + EDGE_LENGTH_EQUALITY_WEIGHT * edgeLengthsEqualityTerm;
            var targetEdgeLengths = QuadraticMinimize(targetEdgeLengthsFunction, scalarVariables);


            var targetLaplacianVectors =
                (from index in allIndices
                 let areaEstimate = Math.Pow(currentEdgeLengths[index], 2)
                 select areaEstimate * currentCurvatures[index] * mesh.Normals[index]
                ).ToArray();
            var targetLaplacianX = (from item in targetLaplacianVectors select (Term)item.X).ToArray();
            var targetLaplacianY = (from item in targetLaplacianVectors select (Term)item.Y).ToArray();
            var targetLaplacianZ = (from item in targetLaplacianVectors select (Term)item.Z).ToArray();

            var freeTargetLaplacianX = ElementsAt(targetLaplacianX, freeIndicesArray);
            var freeTargetLaplacianY = ElementsAt(targetLaplacianY, freeIndicesArray);
            var freeTargetLaplacianZ = ElementsAt(targetLaplacianZ, freeIndicesArray);

            var constrainedTargetLaplacianX = ElementsAt(targetLaplacianX, constrainedIndicesArray);
            var constrainedTargetLaplacianY = ElementsAt(targetLaplacianY, constrainedIndicesArray);
            var constrainedTargetLaplacianZ = ElementsAt(targetLaplacianZ, constrainedIndicesArray);

            var targetEdges = (from edge in topologyInfo.GetEdges()
                               let vi = mesh.Positions[edge.Item1]
                               let vj = mesh.Positions[edge.Item2]
                               let ei = targetEdgeLengths[edge.Item1]
                               let ej = targetEdgeLengths[edge.Item2]
                               select 0.5 * (ei + ej) * (vi - vj) / (vi - vj).Length
                              ).ToArray();
            var targetEdgesX = (from item in targetEdges select (Term)item.X).ToArray();
            var targetEdgesY = (from item in targetEdges select (Term)item.Y).ToArray();
            var targetEdgesZ = (from item in targetEdges select (Term)item.Z).ToArray();

            var constrainedTargetPositionsX = (from item in constrainedPositionsArray select (Term)item.X).ToArray();
            var constrainedTargetPositionsY = (from item in constrainedPositionsArray select (Term)item.Y).ToArray();
            var constrainedTargetPositionsZ = (from item in constrainedPositionsArray select (Term)item.Z).ToArray();

            var positionVariablesX = ArrayUtils.Generate<Variable>(mesh.Positions.Count);
            var positionVariablesY = ArrayUtils.Generate<Variable>(mesh.Positions.Count);
            var positionVariablesZ = ArrayUtils.Generate<Variable>(mesh.Positions.Count);

            var constrainedPositionVariablesX = ElementsAt(positionVariablesX, constrainedIndicesArray);
            var constrainedPositionVariablesY = ElementsAt(positionVariablesY, constrainedIndicesArray);
            var constrainedPositionVariablesZ = ElementsAt(positionVariablesZ, constrainedIndicesArray);

            var edgeVariablesX = 
                (from tuple in topologyInfo.GetEdges() 
                 select positionVariablesX[tuple.Item1] - positionVariablesX[tuple.Item2]
                ).ToArray();
            var edgeVariablesY =
                (from tuple in topologyInfo.GetEdges()
                 select positionVariablesY[tuple.Item1] - positionVariablesY[tuple.Item2]
                ).ToArray();
            var edgeVariablesZ =
                (from tuple in topologyInfo.GetEdges()
                 select positionVariablesZ[tuple.Item1] - positionVariablesZ[tuple.Item2]
                ).ToArray();

            var laplacianVariablesX = GetLaplacians(positionVariablesX, allIndices);
            var laplacianVariablesY = GetLaplacians(positionVariablesY, allIndices);
            var laplacianVariablesZ = GetLaplacians(positionVariablesZ, allIndices);

            var freeLaplacianVariablesX = ElementsAt(laplacianVariablesX, freeIndicesArray);
            var freeLaplacianVariablesY = ElementsAt(laplacianVariablesY, freeIndicesArray);
            var freeLaplacianVariablesZ = ElementsAt(laplacianVariablesZ, freeIndicesArray);

            var constrainedLaplacianVariablesX = ElementsAt(laplacianVariablesX, constrainedIndicesArray);
            var constrainedLaplacianVariablesY = ElementsAt(laplacianVariablesY, constrainedIndicesArray);
            var constrainedLaplacianVariablesZ = ElementsAt(laplacianVariablesZ, constrainedIndicesArray);

            var freePositionLaplacianTerm =
                SquareDiff(freeLaplacianVariablesX, freeTargetLaplacianX) +
                SquareDiff(freeLaplacianVariablesY, freeTargetLaplacianY) +
                SquareDiff(freeLaplacianVariablesZ, freeTargetLaplacianZ);
            var constrainedPositionLaplacianTerm =
                SquareDiff(constrainedLaplacianVariablesX, constrainedTargetLaplacianX) +
                SquareDiff(constrainedLaplacianVariablesY, constrainedTargetLaplacianY) +
                SquareDiff(constrainedLaplacianVariablesZ, constrainedTargetLaplacianZ);
            var positionEqualityTerm =
                SquareDiff(constrainedPositionVariablesX, constrainedTargetPositionsX) +
                SquareDiff(constrainedPositionVariablesY, constrainedTargetPositionsY) +
                SquareDiff(constrainedPositionVariablesZ, constrainedTargetPositionsZ);
            var edgeEqualityTerm =
                SquareDiff(edgeVariablesX, targetEdgesX) +
                SquareDiff(edgeVariablesY, targetEdgesY) +
                SquareDiff(edgeVariablesZ, targetEdgesZ);

            var allTerms = new Term[]
            {
                1.0 * freePositionLaplacianTerm,
                0.1 * constrainedPositionLaplacianTerm,
                1000 * positionEqualityTerm,
                0.001 * edgeEqualityTerm,
            };
            var finalFunction = TermBuilder.Sum(allTerms);
            var allVariables = positionVariablesX.Concat(positionVariablesY).Concat(positionVariablesZ).ToArray();
            var optimalValues = QuadraticMinimize(finalFunction, allVariables, 100);

            for (int i = 0; i < mesh.Positions.Count; ++i)
            {
                var x = optimalValues[i];
                var y = optimalValues[mesh.Positions.Count + i];
                var z = optimalValues[2 * mesh.Positions.Count + i];
                mesh.Positions[i] = new Point3D(x, y, z);
            }
        }

        [Pure]
        private double CalculateArea(Tuple<int, int, int> triangle)
        {
            var v0 = mesh.Positions[triangle.Item1];
            var v1 = mesh.Positions[triangle.Item2];
            var v2 = mesh.Positions[triangle.Item3];

            var cross = Vector3D.CrossProduct(v1 - v0, v2 - v0);
            return 0.5 * cross.Length;
        }

        [Pure]
        private static double[] QuadraticMinimize(Term targetFunction, Variable[] targetCurvatureVariables, double epsilon = 1E-2)
        {
            var x = new double[targetCurvatureVariables.Length];
            var xOld = new double[x.Length];
            for(int i = 0; i < xOld.Length; ++i)
                xOld[i] = double.NaN;

            double diff;
            while (!((diff = SquareDiff(x, xOld)) < epsilon))
            {
                var gradient = Differentiator.Differentiate(targetFunction, targetCurvatureVariables, x);

                var stepSize = CalculateStepSize(
                    function: targetFunction,
                    variables: targetCurvatureVariables,
                    x: x,
                    gradient: gradient);

                xOld = x;
                x = GradientStep(x, gradient, stepSize);
            }

            return x;
        }

        [Pure]
        private static double CalculateStepSize(Term function, Variable[] variables, double[] x, double[] gradient)
        {
            Contract.Requires(function != null);
            Contract.Requires(variables != null);
            Contract.Requires(x.Length == gradient.Length);
            Contract.Requires(x.Length == variables.Length);

            // we use the fact that the original function is quadratic, therefore the line-search
            // function is quadratic in the step size. We will interpolate the step-size function
            // and find the step-size that minimizes it.

            // the vector (x - gradient). This is the input for step size = 1
            var xOne = x.Zip(gradient, (v, g) => v - g).ToArray();

            // the vector (x - 2 * gradient). This is the input for step size = 2
            var xTwo = x.Zip(gradient, (v, g) => v - 2 * g).ToArray();

            var a = Evaluator.Evaluate(function, variables, x);    // evaluate the value for step size = 0
            var b = Evaluator.Evaluate(function, variables, xOne); // evaluate the value for step size = 1
            var c = Evaluator.Evaluate(function, variables, xTwo); // evaluate the value for step size = 2

            // We will calculate the coefficients of g(s) = alpha * s² + beta * s + gamma, where s is the step size.
            // However we do not need gamma for the minimizer.
            var alpha = (a - 2 * b + c) / 2;
            var beta = (-3 * a + 4 * b - c) / 2;

            Contract.Assume(alpha > 0); // g(s) is a strongly convex parabola
            Contract.Assume(beta < 0);  // g(s) has will have a positive minimizer
            
            var minimizer = -beta / (2 * alpha);
            Contract.Assert(minimizer > 0);
            
            return minimizer;
        }

        [Pure]
        private static double[] GradientStep(double[] x, double[] gradient, double stepSize)
        {
            Contract.Requires(x.Length == gradient.Length);
            Contract.Ensures(Contract.Result<double[]>().Length == x.Length);

            var result = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
                result[i] = x[i] - stepSize * gradient[i];

            return result;
        }

        [Pure]
        public static Variable[] GenerateVariables(int count)
        {
            Variable[] variables = new Variable[count];
            for (int i = 0; i < count; ++i)
                variables[i] = new Variable();

            return variables;
        }

        private Term[] ValuesToTerms(double[] values)
        {
            var result = new Term[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = (Term)values[i];

            return result;
        }

        [Pure]
        private Term[] GetLaplacians(Term[] values, int[] indices)
        {
            return GetLaplacians(topologyInfo, values, indices);
        }

        [Pure]
        private double[] GetCurvatureEstimates()
        {
            Contract.Ensures(Contract.Result<double[]>().Length == mesh.Positions.Count);

            var positions = mesh.Positions;
            var normals = mesh.Normals;

            var curvatures =
                (from item in positions.ZipIndex()
                 let x = item.Value
                 let i = item.Index
                 let neighbors = from neighborIndex in topologyInfo.VertexNeighborsOfVertex(i)
                                 select positions[neighborIndex]
                 let laplacian = x - neighbors.Centroid()
                 let normal = normals[i].Normalized()
                 select Vector3D.DotProduct(laplacian, normal)
                )
                .ToArray();

            return curvatures;
        }

        [Pure]
        private static Term SquareDiff(Term[] first, Term[] second)
        {
            Contract.Requires(first.Length == second.Length);

            var diff =
                from pair in first.Zip(second)
                select pair.Item1 - pair.Item2;
            return SumSqr(diff.ToArray());
        }

        [Pure]
        private static double SquareDiff(double[] first, double[] second)
        {
            Contract.Requires(first.Length == second.Length);

            double sum = 0;
            for (int i = 0; i < first.Length; ++i)
                sum += (first[i] - second[i]) * (first[i] - second[i]);
            return sum;
        }

        [Pure]
        private static Term SumSqr(params Term[] terms)
        {
            var squares =
                from term in terms
                select TermBuilder.Power(term, 2);

            return TermBuilder.Sum(squares);
        }

        [Pure]
        private static Term[] GetLaplacians(MeshTopologyInfo topologyInfo, Term[] terms, int[] indices)
        {
            Contract.Requires(topologyInfo != null);
            Contract.Requires(terms != null);
            Contract.Requires(Contract.ForAll(terms, value => value != null));
            Contract.Requires(topologyInfo.VertexCount == terms.Length);
            Contract.Requires(Contract.ForAll(indices, index => index < topologyInfo.VertexCount));

            Contract.Ensures(Contract.Result<Term[]>() != null);
            Contract.Ensures(Contract.ForAll(Contract.Result<Term[]>(), term => term != null));
            Contract.Ensures(Contract.Result<Term[]>().Length == terms.Length);

            var result = new Term[terms.Length];
            for (int k = 0; k < indices.Length; ++k)
            {
                var vertexIndex = indices[k];
                var neighborhoodValues =
                    from neighborIndex in topologyInfo.VertexNeighborsOfVertex(vertexIndex)
                    select terms[neighborIndex];
                var currentValue = terms[k];
                result[k] = currentValue - TermBuilder.Sum(neighborhoodValues);
            }

            return result;
        }

        private static T[] ElementsAt<T>(IList<T> list, int[] indices)
        {
            Contract.Requires(list != null);
            Contract.Requires(indices != null);
            Contract.Requires(Contract.ForAll(indices, index => index < list.Count));
            Contract.Ensures(Contract.Result<T[]>().Length == indices.Length);

            var result = new T[indices.Length];
            for (int i = 0; i < indices.Length; ++i)
                result[i] = list[i];
            return result;
        }
    }
}
