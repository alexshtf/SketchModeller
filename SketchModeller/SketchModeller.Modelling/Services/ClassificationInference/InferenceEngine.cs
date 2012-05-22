using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class InferenceEngine
    {
        public void InferClassification(Graph graph)
        {
            var classifiedNodes =
                from node in graph.Nodes
                where node.Curve.CurveCategory != CurveCategories.None
                select node;

            var bfsQueue = new Queue<Node>();
            foreach (var classifiedNode in classifiedNodes)
                bfsQueue.Enqueue(classifiedNode);
            var visitedNodes = new HashSet<Node>(bfsQueue);

            while (bfsQueue.Count > 0)
            {
                var currentNode = bfsQueue.Dequeue();

                var neighborsToVisit =
                    from neighbor in currentNode.Neighbors
                    where !visitedNodes.Contains(neighbor)
                    where neighbor.Curve.CurveCategory == CurveCategories.None
                    select neighbor;
                neighborsToVisit.ToArray();

                var currCategory = currentNode.Curve.CurveCategory;
                var currClass = currentNode.GeometricClass;
                // line + feature ==> all neighboring lines are features
                if (currClass == GeometricClass.Line && currCategory == CurveCategories.Feature)
                    foreach(var neighbor in neighborsToVisit)
                        if (neighbor.GeometricClass == GeometricClass.Line)
                        {
                            neighbor.Curve.CurveCategory = CurveCategories.Feature;
                            bfsQueue.Enqueue(neighbor);
                            visitedNodes.Add(neighbor);
                        }

                // line + silhouette ==> all neighboring circles are features
                if (currClass == GeometricClass.Line && currCategory == CurveCategories.Silhouette)
                    foreach(var neighbor in neighborsToVisit)
                        if (neighbor.GeometricClass == GeometricClass.Ellipse)
                        {
                            neighbor.Curve.CurveCategory = CurveCategories.Feature;
                            bfsQueue.Enqueue(neighbor);
                            visitedNodes.Add(neighbor);
                        }

                // circle + silhouette ==> all neighboring lines are silhouettes
                if (currClass == GeometricClass.Ellipse && currCategory == CurveCategories.Silhouette)
                    foreach(var neighbor in neighborsToVisit)
                        if (neighbor.GeometricClass == GeometricClass.Line)
                        {
                            neighbor.Curve.CurveCategory = CurveCategories.Silhouette;
                            bfsQueue.Enqueue(neighbor);
                            visitedNodes.Add(neighbor);
                        }

                // circle + feature ==> all neighboring lines are silhouettes
                if (currClass == GeometricClass.Ellipse && currCategory == CurveCategories.Feature)
                    foreach(var neighbor in neighborsToVisit)
                        if (neighbor.GeometricClass == GeometricClass.Line)
                        {
                            neighbor.Curve.CurveCategory = CurveCategories.Silhouette;
                            bfsQueue.Enqueue(neighbor);
                            visitedNodes.Add(neighbor);
                        }
            }
        }
    }
}
