using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils;
using SketchModeller.Utilities;

using Enumerable = System.Linq.Enumerable;

namespace SketchModeller.Modelling.Computations
{
    static class StraightSpine
    {
        public static Tuple<double[], Point, Point> Compute(Point[] l1pts, Point[] l2pts, double[] progress)
        {
            var subdivision = CurvedSubdividor.Subdivide(l1pts, l2pts);
            var spineLine = PCALine.Compute(subdivision.SpinePoints);
            var spineProjections =
                from pnt in subdivision.SpinePoints
                let t = pnt.ProjectOnLine(spineLine.Item1, spineLine.Item2)
                select t;
            spineProjections = spineProjections.ToArray();

            var progressProjections = ComputeProgressProjections(progress, spineProjections);
            var radii = ComputeRadii(l1pts, l2pts, spineLine, progressProjections);

            var pStart = spineLine.Item1 + spineProjections.Min() * spineLine.Item2;
            var pEnd = spineLine.Item1 + spineProjections.Max() * spineLine.Item2;

            return Tuple.Create(radii, pStart, pEnd);
        }

        private static IEnumerable<double> ComputeProgressProjections(double[] progress, IEnumerable<double> spineProjections)
        {
            var spineProjMin = spineProjections.Min();
            var spineProjMax = spineProjections.Max();
            var progressProjections =
                from t in progress
                select spineProjMin + t * (spineProjMax - spineProjMin);
            progressProjections.ToArray();
            return progressProjections;
        }

        private static double[] ComputeRadii(
           Point[] l1pts, Point[] l2pts,
           Tuple<Point, Vector> spineLine,
           IEnumerable<double> spineProjections)
        {
            var spineLineNormal = new Vector(-spineLine.Item2.Y, spineLine.Item2.X).Normalized();
            var straightSpinePoints =
                (from t in spineProjections
                 select spineLine.Item1 + t * spineLine.Item2).ToArray();
            var straightSpineNormals =
                Enumerable.Repeat(spineLineNormal, straightSpinePoints.Length)
                .ToArray();

            var radii = CurvedSubdividor.ComputeRadii(
                straightSpinePoints,
                straightSpineNormals,
                l1pts,
                l2pts);
            return radii;
        }
    }
}
