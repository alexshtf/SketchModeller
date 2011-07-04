using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics.Contracts;
using SketchModeller.Utilities;

namespace SketchModeller.Modelling.Computations
{
    static class ProximityDistanceEstimate
    {
        private static readonly Random random = new Random();

        public static double Compute(Point[] polylinePoints, double percentile = 0.05)
        {
            Contract.Requires(polylinePoints != null);
            Contract.Requires(polylinePoints.Length > 0);
            Contract.Ensures(Contract.Result<double>() >= 0);

            var n = polylinePoints.Length;
            var sampleSize = (int)(n * Math.Log(n) * Math.Log(n));
            var distancesSample = 
                from i in Enumerable.Range(0, sampleSize)
                let s1 = random.Next(n)
                let s2 = random.Next(n)
                let distance = (polylinePoints[s1] - polylinePoints[s2]).Length
                orderby distance ascending
                select distance;

            var result = distancesSample.ElementAt((int)(percentile * sampleSize));
            return result;
        }
    }
}
