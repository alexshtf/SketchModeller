using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Utilities;
using System.Windows;
using System.Windows.Media.Media3D;
using AutoDiff;
using System.Diagnostics.Contracts;

namespace SketchModeller.Modelling.Services.Snap
{
    static class SnapperHelper
    {
        private static Term MeanSquaredError(Term[] terms, double[] values)
        {
            Contract.Requires(terms != null);
            Contract.Requires(values != null);
            Contract.Requires(terms.Length == values.Length);
            Contract.Requires(Contract.ForAll(terms, term => term != null));
            Contract.Ensures(Contract.Result<Term>() != null);

            var errorTerms = from i in Enumerable.Range(0, terms.Length)
                             select TermBuilder.Power(terms[i] + (-values[i]), 2);

            return (1 / (double)terms.Length) * TermUtils.SafeSum(errorTerms);
        }

        public static double EstimateRadius(Point3D center, IEnumerable<Point3D> proj)
        {
            var radii = proj.Select(x => (center - x).Length);
            return radii.Average();
        }

        public static TVec GenerateVarVector()
        {
            return new TVec(new Variable(), new Variable(), new Variable());
        }

        public static Point3D[] CirclePoints(Point3D center, Vector3D u, Vector3D v, double radius, int count)
        {
            var circlePoints = new Point3D[count];
            for (int i = 0; i < count; ++i)
            {
                var fraction = i / (double)count;
                var angle = 2 * Math.PI * fraction;
                circlePoints[i] = center + radius * (u * Math.Cos(angle) + v * Math.Sin(angle));
            }
            return circlePoints;
        }

        /// <summary>
        /// Checks weather a points sequence is the top-part of a cylinder, by comparing the center of the ellipse that best fits the points
        /// sequence to the actual top center and bottom centers.
        /// </summary>
        /// <param name="points">The points sequence</param>
        /// <param name="cylinder">The cylinder data</param>
        /// <returns><c>true</c> if the best-fit-ellipse's center is closer to the top cylinder point than to the bottom.</returns>
        public static bool IsTop(PointsSequence points, dynamic cylinder)
        {
            var top = new Point(cylinder.TopCenterResult.X, -cylinder.TopCenterResult.Y);
            var bottom = new Point(cylinder.BottomCenterResult.X, -cylinder.BottomCenterResult.Y);

            var samples = CurveSampler.UniformSample(points, 50);
            var ellipse = EllipseFitter.Fit(samples);

            // the points sequence is "top" if it is closer to the top center then it is to the bottom center.
            if ((top - ellipse.Center).Length < (bottom - ellipse.Center).Length)
                return true;
            else
                return false;
        }
    }
}
