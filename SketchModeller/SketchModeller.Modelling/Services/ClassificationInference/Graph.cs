using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class Graph
    {
        public readonly Node[] Nodes;

        public Graph(Node[] nodes)
        {
            this.Nodes = nodes;
        }
    }

    class Node
    {
        public GeometricClass GeometricClass;
        public readonly PointsSequence Curve;
        public readonly HashSet<Node> Neighbors;

        public Node(PointsSequence p, GeometricClass geometricClass)
        {
            this.Curve = p;
            this.GeometricClass = geometricClass;
            this.Neighbors = new HashSet<Node>();
        }

    }
}
