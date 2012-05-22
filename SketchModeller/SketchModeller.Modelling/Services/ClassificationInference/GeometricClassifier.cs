using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Modelling.Computations;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class GeometricClassifier
    {
        private readonly double pcaFractionThreshold;

        public GeometricClassifier(double pcaFractionThreshold)
        {
            this.pcaFractionThreshold = pcaFractionThreshold;
        }

        public GeometricClass Classify(PointsSequence p)
        {
            var pca = PointsPCA2D.Compute(p.Points);
            var fraction = pca.Second.Length / pca.First.Length;

            if (fraction < pcaFractionThreshold)
                return GeometricClass.Line;
            else
                return GeometricClass.Ellipse;
        }
    }
}
