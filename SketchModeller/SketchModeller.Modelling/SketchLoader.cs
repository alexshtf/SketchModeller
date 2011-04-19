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

namespace SketchModeller.Modelling
{
    class SketchLoader
    {
        private IEventAggregator eventAggregator;
        private ISketchCatalog sketchCatalog;
        private SessionData sessionData;
        private UiState uiState;

        [InjectionConstructor]
        public SketchLoader(IEventAggregator eventAggregator, ISketchCatalog sketchCatalog, SessionData sessionData, UiState uiState)
        {
            this.eventAggregator = eventAggregator;
            this.sketchCatalog = sketchCatalog;
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
            var sketchData = tuple.Item1;
            var sketchName = tuple.Item2;

            sessionData.SketchName = sketchName;
            sessionData.SketchData = sketchData;

            sessionData.NewPrimitives.Clear();
            sessionData.SnappedPrimitives.Clear();
            sessionData.Annotations.Clear();

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
            
            uiState.SketchPlane = uiState.SketchPlanes[0];
            while (uiState.SketchPlanes.Count > 1)
                uiState.SketchPlanes.RemoveAt(1);
        }
    }
}
