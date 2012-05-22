using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure.Shared;

namespace SketchModeller.Modelling.Services.ClassificationInference
{
    class ClassificationInferneceService : IClassificationInference
    {
        private SessionData sessionData;
        private GraphConstructor graphConstructor;
        private InferenceEngine inferenceEngine;
        private Graph graph;

        public ClassificationInferneceService(SessionData sessionData)
        {
            this.sessionData = sessionData;

            var geometricClassifier = new GeometricClassifier();
            var neighborhoodChecker = new NeighborhoodChecker();

            this.graphConstructor = new GraphConstructor(geometricClassifier, neighborhoodChecker);
            this.inferenceEngine = new InferenceEngine();
        }

        public void PreAnalyze()
        {
            graph = graphConstructor.Construct(sessionData.SketchObjects);
        }

        public void Infer()
        {
            inferenceEngine.InferClassification(graph);
        }
    }
}
