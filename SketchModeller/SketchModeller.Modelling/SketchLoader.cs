using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Data;
using Utils;
using SketchModeller.Infrastructure;
using System.IO;
using System.Xml.Serialization;
using System.Reactive.Linq;
using SketchModeller.Utilities.Debugging;

namespace SketchModeller.Modelling
{
    class SketchLoader
    {
        private IEventAggregator eventAggregator;
        private ISketchCatalog sketchCatalog;
        private IClassificationInference classificationInference;
        private readonly IUndoHistory undoHistory;
        private SessionData sessionData;
        private UiState uiState;

        [InjectionConstructor]
        public SketchLoader(IEventAggregator eventAggregator,
                            ISketchCatalog sketchCatalog, 
                            IClassificationInference classificationInference,
                            IUndoHistory undoHistory,
                            SessionData sessionData, 
                            UiState uiState)
        {
            this.eventAggregator = eventAggregator;
            this.sketchCatalog = sketchCatalog;
            this.classificationInference = classificationInference;
            this.undoHistory = undoHistory;
            this.sessionData = sessionData;
            this.uiState = uiState;

            eventAggregator.GetEvent<LoadSketchEvent>().Subscribe(OnLoadSketch);
        }

        private void OnLoadSketch(string sketchName)
        {
            Work.Execute<Tuple<SketchData, string>>(
                eventAggregator, 
                workItem: () => (from sketchData in sketchCatalog.LoadSketchAsync(sketchName)
                                 select Tuple.Create(sketchData, sketchName)), 
                onNext: OnSketchLoaded);
        }

        private void OnSketchLoaded(Tuple<SketchData, string> tuple)
        {
            undoHistory.Clear();

            var sketchData = tuple.Item1;
            var sketchName = tuple.Item2;

            sessionData.SketchName = sketchName;
            sessionData.SketchData = sketchData;

            sessionData.NewPrimitives.Clear();
            sessionData.SnappedPrimitives.Clear();
            sessionData.Annotations.Clear();
            sessionData.FeatureCurves.Clear();

            if (sketchData.NewPrimitives != null)
                sessionData.NewPrimitives.AddRange(sketchData.NewPrimitives);
            if (sketchData.SnappedPrimitives != null)
            {
                sessionData.SnappedPrimitives.AddRange(sketchData.SnappedPrimitives);
                sessionData.FeatureCurves.AddRange(sketchData.SnappedPrimitives.SelectMany(sp => sp.FeatureCurves));
            }
            if (sketchData.Annotations != null)
                sessionData.Annotations.AddRange(sketchData.Annotations);

            var curves = sketchData.Curves ?? System.Linq.Enumerable.Empty<PointsSequence>();
            sessionData.SketchObjects = curves.ToArray();
            foreach (var item in sessionData.SketchObjects)
                item.ColorCodingIndex = PointsSequence.INVALID_COLOR_CODING;

            sessionData.DistanceTransforms = sketchData.DistanceTransforms.ToArray();
            classificationInference.PreAnalyze();
            //if (sketchData.ConnectivityGraph == null)
            //    sessionData.ConnectivityGraph = ComputeConnectivityGraph(sessionData.SketchObjects);
            //else
            //    sessionData.ConnectivityGraph = sketchData.ConnectivityGraph;

            //=====================================================================================
            // the following code saves the distance transform to images. Uncomment for debugging
            // and illustration purposes. Please edit the path before running.
            //=====================================================================================
            //for (int i = 0; i < sessionData.DistanceTransforms.Length; ++i)
            //{
            //    var dt = sessionData.DistanceTransforms[i];
            //    var w = dt.GetLength(0);
            //    var h = dt.GetLength(1);
            //    var doubleDt = new double[h, w];
            //    for(int x = 0; x < w; ++x)
            //        for(int y = 0; y < h; ++y)
            //            doubleDt[y, x] = dt[x, y];

            //    ArrayImage.SaveScaledGray(doubleDt, "C:\\Users\\Alex\\Desktop\\dts\\dt" + i + ".png");
            //}
            //=====================================================================================
            
            uiState.SketchPlane = uiState.SketchPlanes[0];
            while (uiState.SketchPlanes.Count > 1)
                uiState.SketchPlanes.RemoveAt(1);
        }
    }
}
