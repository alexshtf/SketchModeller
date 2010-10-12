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
            var scalarLaplacianTerm = SumSqr(GetLaplacians(scalarVariables, allIndices));


            var currentCurvatures = GetCurvatureEstimates();
            var curvaturesEqualityTerm = SquareDiff(scalarVariables, ValuesToTerms(currentCurvatures));
            var targetCurvaturesFunction = 1.0 * scalarLaplacianTerm + 1.0 * curvaturesEqualityTerm;
            var targetCurvatures = QuadraticMinimize(targetCurvaturesFunction, scalarVariables, initial: currentCurvatures, epsilon: 1E-4);


            var currentEdgeLengths =
                (from index in allIndices
                 let currentPosition = mesh.Positions[index]
                 let edgeLengths = from neighborIndex in topologyInfo.VertexNeighborsOfVertex(index)
                                   let neighborPosition = mesh.Positions[neighborIndex]
                                   select (currentPosition - neighborPosition).Length
                 select edgeLengths.Average()
                ).ToArray();
            var edgeLengthsEqualityTerm = SquareDiff(scalarVariables, ValuesToTerms(currentEdgeLengths));
            var targetEdgeLengthsFunction = 1.0 * scalarLaplacianTerm + 0.1 * edgeLengthsEqualityTerm;
            var targetEdgeLengths = QuadraticMinimize(targetEdgeLengthsFunction, scalarVariables, initial: currentEdgeLengths, epsilon: 1E-4);


            var targetLaplacianVectors =
                (from index in allIndices
                 let areaEstimate = 1.0 //Math.Pow(currentEdgeLengths[index], 2)
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
                TermBuilder.Constant(0),
                1.0    * freePositionLaplacianTerm,
                0.9    * constrainedPositionLaplacianTerm,
                100.0  * positionEqualityTerm,
                0.001  * edgeEqualityTerm,
            };
            var finalFunction = TermBuilder.Sum(allTerms);
            var allVariables = positionVariablesX.Concat(positionVariablesY).Concat(positionVariablesZ).ToArray();
            var currentPositionsArray =
                (from item in mesh.Positions select item.X).Concat(
                 from item in mesh.Positions select item.Y).Concat(
                 from item in mesh.Positions select item.Z).ToArray();
            var optimalValues = QuadraticMinimize(finalFunction, allVariables, epsilon: 100, initial: currentPositionsArray);

            for (int i = 0; i < mesh.Positions.Count; ++i)
            {
                var x = optimalValues[0 * mesh.Positions.Count + i];
                var y = optimalValues[1 * mesh.Positions.Count + i];
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
        private static double[] QuadraticMinimize(Term targetFunction, Variable[] variables, double epsilon = 1E-2, double[] initial = null)
        {
            var minimizer = new QuadraticOptimizer(targetFunction, variables);
            foreach (var optimizationResult in minimizer.Minimize(initial))
            {
                var diff = SquareDiff(optimizationResult.CurrentMinimizer, optimizationResult.PrevMinimizer);
                if (diff < epsilon)
                    return optimizationResult.CurrentMinimizer;
            }

            Debug.Fail("We should have not reached here.");
            return null;
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
                    (from neighborIndex in topologyInfo.VertexNeighborsOfVertex(vertexIndex)
                     select terms[neighborIndex]
                    ).ToArray();
                var currentValue = terms[k];
                var factor = -1.0 / neighborhoodValues.Length;
                result[k] = currentValue + factor * TermBuilder.Sum(neighborhoodValues);
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
                result[i] = list[indices[i]];
            return result;
        }
    }
}
