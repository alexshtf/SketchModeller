using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class NeighborhoodChecker
    {
        private double relativeThreshold;

        public NeighborhoodChecker(double relativeThreshold)
        {
            this.relativeThreshold = relativeThreshold;
        }

        public bool AreNeighbors(Node first, Node second)
        {
            if (first.GeometricClass == GeometricClass.Ellipse && second.GeometricClass == GeometricClass.Ellipse)
                return false;

            if (first.GeometricClass == GeometricClass.Ellipse && second.GeometricClass == GeometricClass.Line)
                return AreNeighborsEllipseLine(ellipse: first, line: second);

            if (first.GeometricClass == GeometricClass.Line && second.GeometricClass == GeometricClass.Ellipse)
                return AreNeighborsEllipseLine(ellipse: second, line: first);

            if (first.GeometricClass == GeometricClass.Line && second.GeometricClass == GeometricClass.Line)
                return AreNeighborsTwoLines(first, second);

            throw new InvalidOperationException();
        }

        private bool AreNeighborsTwoLines(Node firstLine, Node secondLine)
        {
            var distances = new double[] 
            {
                (firstLine.Curve.Points.First() - secondLine.Curve.Points.First()).Length,
                (firstLine.Curve.Points.First() - secondLine.Curve.Points.Last()).Length,
                (firstLine.Curve.Points.Last() - secondLine.Curve.Points.First()).Length,
                (firstLine.Curve.Points.Last() - secondLine.Curve.Points.Last()).Length,
            };
            var minDistance = distances.Min();

            var sizes = new double[] 
            {
                (firstLine.Curve.Points.First() - firstLine.Curve.Points.Last()).Length,
                (secondLine.Curve.Points.First() - secondLine.Curve.Points.Last()).Length,
            };
            var maxSize = sizes.Max();

            var fraction = minDistance / maxSize;
            return fraction < relativeThreshold;
        }

        private bool AreNeighborsEllipseLine(Node ellipse, Node line)
        {
            var distances =
                from point in line.Curve.Points
                select point.ProjectionOnCurve(ellipse.Curve.Points).Item2;
            var minDistance = distances.Min();


            var ellipseDistances =
                from p in ellipse.Curve.Points
                from q in ellipse.Curve.Points
                select (p - q).Length;
            var ellipseSize = ellipseDistances.Max();
            var lineSize = (line.Curve.Points.First() - line.Curve.Points.Last()).Length;
            var maxSize = Math.Max(ellipseSize, lineSize);

            var fraction = minDistance / maxSize;
            return fraction < relativeThreshold;
        }
    }
}
