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
            if (sketchData.Cylinders != null)
            {
                foreach (var item in sketchData.Cylinders)
                {
                    sessionData.NewPrimitives.Add(new NewCylinder
                    {
                        Axis = item.Axis,
                        Center = item.Center,
                        Length = item.Length,
                        Diameter = item.Diameter,
                    });
                }
            }

            var imWidth = sketchData.Image.GetLength(0);
            var imHeight = sketchData.Image.GetLength(1);
            
            uiState.SketchPlane = uiState.SketchPlanes[0];
            while (uiState.SketchPlanes.Count > 1)
                uiState.SketchPlanes.RemoveAt(1);
        }
    }
}
