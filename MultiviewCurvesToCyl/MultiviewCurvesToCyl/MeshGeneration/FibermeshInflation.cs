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
using Bluebit.MatrixLibrary;

namespace MultiviewCurvesToCyl.MeshGeneration
{
    class FibermeshInflation
    {
        private readonly ConstrainedMesh mesh;
        private readonly MeshTopologyInfo topologyInfo;
        private readonly int[] allIndices;
        private readonly int[] constrainedIndicesArray;

        private readonly Variable[] scalarVariables;
        private readonly Term scalarLaplacianTerm;

        private readonly Variable[] positionVariablesX;
        private readonly Variable[] positionVariablesY;
        private readonly Variable[] positionVariablesZ;
        private readonly Variable[] allPositionVariables;

        private readonly SparseSolver curvaturesSolver;
        private readonly SparseSolver edgeLengthSolver;
        private readonly SparseSolver positionSolver;

        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(mesh != null);
            Contract.Invariant(topologyInfo != null);
        }

        public FibermeshInflation(ConstrainedMesh mesh, int[] constrainedIndices)
        {
            Contract.Requires(mesh != null);

            this.mesh = mesh;
            this.topologyInfo = new MeshTopologyInfo(mesh.TriangleIndices);
            this.constrainedIndicesArray = constrainedIndices;

            var vertexCount = mesh.Positions.Count;
            allIndices = System.Linq.Enumerable.Range(0, vertexCount).ToArray();

            scalarVariables = ArrayUtils.Generate<Variable>(vertexCount);
            positionVariablesX = ArrayUtils.Generate<Variable>(vertexCount);
            positionVariablesY = ArrayUtils.Generate<Variable>(vertexCount);
            positionVariablesZ = ArrayUtils.Generate<Variable>(vertexCount);
            allPositionVariables = 
                positionVariablesX.Concat(
                positionVariablesY).Concat(
                positionVariablesZ).ToArray();

            scalarLaplacianTerm = SumSqr(GetLaplacians(scalarVariables, allIndices));

            // we will now generate target functions based on fake data
            // we know that the quadratic factors of the target functions don't change and therefore
            // the matrix part of the quadratic functions stays the same. So we can construct solvers
            // that will efficiently solve our optimization problems
            var fakeCurvatures = System.Linq.Enumerable.Repeat(1.0, vertexCount).ToArray();
            var fakeEdgeLengths = System.Linq.Enumerable.Repeat(1.0, vertexCount).ToArray();
            var fakeConstrainedPositions = ElementsAt(mesh.Positions, constrainedIndicesArray);

            var fakeCurvaturesFunc = CreateCurvaturesFunction(fakeCurvatures);
            var fakeEdgeLengthFunc = CreateEdgeLengthsFunction(fakeEdgeLengths);
            var fakePositionFunc = CreatePositionsFunction(fakeCurvatures, fakeEdgeLengths, fakeConstrainedPositions);

            curvaturesSolver = GetSolver(fakeCurvaturesFunc, scalarVariables);
            edgeLengthSolver = GetSolver(fakeEdgeLengthFunc, scalarVariables);
            positionSolver = GetSolver(fakePositionFunc, allPositionVariables);
        }

        private SparseSolver GetSolver(Term fakeCurvaturesFunc, Variable[] variables)
        {
            var quadraticFactorsData = QuadraticFunctionHelper.ExtractQuadraticFactors(fakeCurvaturesFunc, variables);

            var sparseMatrix = new SparseMatrix(variables.Length, variables.Length);
            foreach (var item in quadraticFactorsData.QuadraticFactors)
            {
                var row = item.Item1;
                var col = item.Item2;
                var val = item.Item3;

                sparseMatrix[row, col] = val;
            }

            var solver = new SparseSolver(sparseMatrix);
            return solver;
        }

        public void SmoothStep(Point3D[] constrainedPositionsArray)
        {
            Contract.Assume(constrainedIndicesArray.Length == constrainedPositionsArray.Length);

            // calculate the target curvatures that we will strive to have after smoothing step
            var currentCurvatures = GetCurvatureEstimates();
            var targetCurvaturesFunction = CreateCurvaturesFunction(currentCurvatures);
            var targetCurvatures = FindMinimum(targetCurvaturesFunction, scalarVariables, curvaturesSolver);

            // calculate target edge lengths that we will strive to have after smoothing step
            var currentEdgeLengths = GetEdgeLengthsEstimate();
            var targetEdgeLengthsFunction = CreateEdgeLengthsFunction(currentEdgeLengths);
            var targetEdgeLengths = FindMinimum(targetEdgeLengthsFunction, scalarVariables, edgeLengthSolver);

            // calculate the positions
            var positionsFunction = CreatePositionsFunction(targetCurvatures, targetEdgeLengths, constrainedPositionsArray);
            var optimalPositions = FindMinimum(positionsFunction, allPositionVariables, positionSolver);

            // put the optimal positions back to the mesh
            for (int i = 0; i < mesh.Positions.Count; ++i)
            {
                var x = optimalPositions[0 * mesh.Positions.Count + i];
                var y = optimalPositions[1 * mesh.Positions.Count + i];
                var z = optimalPositions[2 * mesh.Positions.Count + i];
                mesh.Positions[i] = new Point3D(x, y, z);
            }
        }

        private double[] FindMinimum(Term targetFunction, Variable[] variables, SparseSolver linearSolver)
        {
            // to minimize <x, Ax> + <b, x> + c we need to solve the system
            // Ax = -0.5 * b. So we will construct the vector -0.5*b and use the solver for A
            // to get the solution.

            var linearData = QuadraticFunctionHelper.ExtractLinearFactors(targetFunction, variables);
            var vec = new Vector(variables.Length);
            foreach (var item in linearData.LinearFactors)
            {
                var index = item.Item1;
                var value = item.Item2;
                vec[index] = -0.5 * value;
            }

            var solution = linearSolver.Solve(vec);
            var result = solution.ToArray();

            return result;
        }

        #region Target functions creation

        private Term CreatePositionsFunction(double[] targetCurvatures, double[] targetEdgeLengths, Point3D[] constrainedPositionsArray)
        {
            var targetLaplacianVectors =
                (from index in allIndices
                 let areaEstimate = 1.0 //Math.Pow(currentEdgeLengths[index], 2)
                 select areaEstimate * targetCurvatures[index] * mesh.Normals[index]
                ).ToArray();
            var targetLaplacianX = (from item in targetLaplacianVectors select (Term)item.X).ToArray();
            var targetLaplacianY = (from item in targetLaplacianVectors select (Term)item.Y).ToArray();
            var targetLaplacianZ = (from item in targetLaplacianVectors select (Term)item.Z).ToArray();

            var freeIndicesArray =
                allIndices
                .Except(new HashSet<int>(constrainedIndicesArray))
                .ToArray();

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
            return finalFunction;
        }

        private Term CreateCurvaturesFunction(double[] currentCurvatures)
        {
            var curvaturesEqualityTerm = SquareDiff(scalarVariables, ValuesToTerms(currentCurvatures));
            var targetCurvaturesFunction = 1.0 * scalarLaplacianTerm + 1.0 * curvaturesEqualityTerm;

            return targetCurvaturesFunction;
        }

        private Term CreateEdgeLengthsFunction(double[] currentEdgeLengths)
        {
            var edgeLengthsEqualityTerm = SquareDiff(scalarVariables, ValuesToTerms(currentEdgeLengths));
            var targetEdgeLengthsFunction = 1.0 * scalarLaplacianTerm + 0.1 * edgeLengthsEqualityTerm;

            return targetEdgeLengthsFunction;
        }

        #endregion

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
        private double[] GetEdgeLengthsEstimate()
        {
            var currentEdgeLengths =
                (from index in allIndices
                 let currentPosition = mesh.Positions[index]
                 let edgeLengths = from neighborIndex in topologyInfo.VertexNeighborsOfVertex(index)
                                   let neighborPosition = mesh.Positions[neighborIndex]
                                   select (currentPosition - neighborPosition).Length
                 select edgeLengths.Average()
                ).ToArray();
            return currentEdgeLengths;
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

        [Pure]
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
