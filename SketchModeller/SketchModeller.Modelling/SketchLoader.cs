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
            Work.Execute<SketchData>(
                eventAggregator, 
                workItem: () => sketchCatalog.LoadSketchAsync(sketchName), 
                onNext: UpdateSketchData);
        }

        private void UpdateSketchData(SketchData sketchData)
        {
            sessionData.SketchData = sketchData;
            var imWidth = sketchData.Image.GetLength(0);
            var imHeight = sketchData.Image.GetLength(1);
            uiState.SketchPlane = new SketchPlane(
                center: MathUtils3D.Origin,
                xAxis: MathUtils3D.UnitX,
                yAxis: MathUtils3D.UnitY,
                normal: MathUtils3D.UnitZ,
                width: imWidth,
                height: imHeight);
        }
    }
}
