using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class GraphConstructor
    {
        private readonly GeometricClassifier geometricClassifier;
        private readonly NeighborhoodChecker neighborhoodChecker;

        public GraphConstructor(GeometricClassifier geometricClassifier, NeighborhoodChecker neighborhoodChecker)
        {
            this.geometricClassifier = geometricClassifier;
            this.neighborhoodChecker = neighborhoodChecker;
        }

        public Graph Construct(PointsSequence[] sketchCurves)
        {
            var nodesQuer =
                from sketchCurve in sketchCurves
                let geometricClass = geometricClassifier.Classify(sketchCurve)
                select new Node(sketchCurve, geometricClass);
            var nodes = nodesQuer.ToArray();

            var allPairs = from first in nodes
                           from second in nodes
                           select Tuple.Create(first, second);
            allPairs = allPairs.ToArray();

            var neighbors =
                from pair in allPairs.AsParallel()
                let first = pair.Item1
                let second = pair.Item2
                where AreNeighbors(first, second)
                select Tuple.Create(first, second);

            foreach (var pair in neighbors.ToArray())
            {
                pair.Item1.Neighbors.Add(pair.Item2);
                pair.Item2.Neighbors.Add(pair.Item1);
            }

            return new Graph(nodes);
        }

        private bool AreNeighbors(Node first, Node second)
        {
            return neighborhoodChecker.AreNeighbors(first, second);
        }
    }
}
