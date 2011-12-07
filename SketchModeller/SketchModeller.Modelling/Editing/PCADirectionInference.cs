using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using SketchModeller.Modelling.Computations;

namespace SketchModeller.Modelling.Editing
{
    public class PCADirectionInference : IDirectionInference
    {
        private readonly double tolerance;
        private List<Point> points;

        public PCADirectionInference(double tolerance = 10)
        {
            this.tolerance = tolerance;
            points = new List<Point>();
        }

        public void Reset()
        {
            points.Clear();
        }

        public void ProvidePoint(Point pnt)
        {
            points.Add(pnt);
        }

        public Vector? InferDirection()
        {
            var pcaResult = PointsPCA2D.Compute(points);
            var firstNorm = pcaResult.First.Length;
            var secondNorm = pcaResult.Second.Length;
            if (firstNorm / secondNorm > tolerance)
                return pcaResult.First;
            else
                return null;
        }
    }
}
