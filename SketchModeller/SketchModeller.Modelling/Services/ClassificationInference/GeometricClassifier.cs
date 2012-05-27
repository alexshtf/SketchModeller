using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Modelling.Computations;
using System.Windows;
using Meta.Numerics.Statistics;
using SketchModeller.Utilities;
using System.Diagnostics;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class GeometricClassifier
    {
        private readonly double pcaFractionThreshold;
        private readonly double ellipseFitThreshold;

        public GeometricClassifier(double pcaFractionThreshold, double ellipseFitThreshold)
        {
            this.pcaFractionThreshold = pcaFractionThreshold;
            this.ellipseFitThreshold = ellipseFitThreshold;
        }

        public GeometricClass Classify(PointsSequence p)
        {
            var points = p.Points;
            var xAvg = points.Select(pnt => pnt.X).Average();
            var yAvg = points.Select(pnt => pnt.Y).Average();

            var shiftedPoints =
                from pnt in points
                select new Point(pnt.X - xAvg, pnt.Y - yAvg);

            var mvSample = new MultivariateSample(2);
            foreach (var pnt in shiftedPoints)
                mvSample.Add(pnt.X, pnt.Y);

            var pca = mvSample.PrincipalComponentAnalysis();
            var firstSize = pca.Component(0).ScaledVector().Norm();
            var secondSize = pca.Component(1).ScaledVector().Norm();

            var fraction = secondSize / firstSize;

            if (fraction < pcaFractionThreshold)
                return GeometricClass.Line;
            else
            {
                var conicEllipse = EllipseFitter.ConicFit(p.Points);
                var parametricEllipse = EllipseFitter.Fit(p.Points);

                var deviations =
                    from pnt in p.Points
                    select EllipseFitter.ComputeDeviation(conicEllipse, parametricEllipse, pnt);
                var avgDeviation = deviations.Average();

                if (avgDeviation < ellipseFitThreshold)
                    return GeometricClass.Ellipse;
                else
                    return GeometricClass.Line;
            }
        }
    }
}
