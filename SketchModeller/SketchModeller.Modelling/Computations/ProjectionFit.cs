using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoDiff;
using System.Windows;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Computations
{
    static class ProjectionFit
    {
        public static IEnumerable<Term> Compute(CircleFeatureCurve item)
        {
            const int SAMPLE_SIZE = 20;
            var sample = CurveSampler.UniformSample(item.SnappedTo, SAMPLE_SIZE);
            return Compute(item, sample);
        }

        /// <summary>
        /// Generates a set of terms, one for each given 2D point, that measure the fitness of each point to being
        /// a 2D projection of the given set.
        /// </summary>
        /// <param name="pointsSet">A representation for the 3D points set</param>
        /// <param name="sample">The set of 2D points</param>
        /// <returns>The set of terms, one for each point in <paramref name="sample"/> that measures the fitness of each such point
        /// to the set in <paramref name="pointsSet"/>.</returns>
        public static IEnumerable<Term> Compute(CircleFeatureCurve pointsSet, Point[] sample)
        {
            var terms =
                from point in sample
                select Compute(pointsSet, point);

            return terms;
        }

        /// <summary>
        /// Generates a term that gets smaller as the given 2D point fits a 3D points set projection.
        /// </summary>
        /// <param name="pointsSet">A representation for the 3D points set</param>
        /// <param name="point">The 2D point</param>
        /// <returns>The term that measures fitness of <paramref name="point"/> being on the 2D projection of the set specified by <paramref name="pointsSet"/></returns>
        public static Term Compute(CircleFeatureCurve pointsSet, Point point)
        {
            // here we explicitly assume that the view vector is (0, 0, 1) or (0, 0, -1)
            var x_ = point.X;
            var y_ = point.Y;

            var cx = pointsSet.Center.X;
            var cy = pointsSet.Center.Y;
            var cz = pointsSet.Center.Z;

            var nx = pointsSet.Normal.X;
            var ny = pointsSet.Normal.Y;
            var nz = pointsSet.Normal.Z;

            var r = pointsSet.Radius;

            var dx = cx - x_;
            var dy = cy + y_;

            var lhs = TermBuilder.Sum(
                TermBuilder.Power(dx * nz, 2),
                TermBuilder.Power(dy * nz, 2),
                TermBuilder.Power(dx * nx + dy * ny, 2));
            var rhs = TermBuilder.Power(r * nz, 2);

            return TermBuilder.Power(lhs - rhs, 2);
        }
    }
}
