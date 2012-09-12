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
        private static readonly string PreprocessingDataKey = typeof (ClassificationInferneceService).FullName;

        private readonly SessionData sessionData;
        private readonly GraphConstructor graphConstructor;
        private readonly InferenceEngine inferenceEngine;

        public ClassificationInferneceService(SessionData sessionData)
        {
            this.sessionData = sessionData;

            var geometricClassifier = new GeometricClassifier(pcaFractionThreshold: 0.1, ellipseFitThreshold: 0.02);
            var neighborhoodChecker = new NeighborhoodChecker(relativeThreshold: 0.05);

            this.graphConstructor = new GraphConstructor(geometricClassifier, neighborhoodChecker);
            this.inferenceEngine = new InferenceEngine();
        }

        public void PreAnalyze()
        {
            if (sessionData.PreprocessingData.ContainsKey(PreprocessingDataKey))
                return;

            var graph = graphConstructor.Construct(sessionData.SketchObjects);
            sessionData.PreprocessingData[PreprocessingDataKey] = graph;
        }

        public void Infer()
        {
            var graph = (Graph)sessionData.PreprocessingData[PreprocessingDataKey];
            inferenceEngine.InferClassification(graph);
        }
    }
}
