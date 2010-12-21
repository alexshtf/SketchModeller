using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Events;
using SketchModeller.Infrastructure.Shared;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Infrastructure.Events;
using SketchModeller.Infrastructure.Data;
using SketchModeller.Infrastructure;

namespace SketchModeller.Modelling
{
    class SketchSaver
    {
        private IEventAggregator eventAggregator;
        private SessionData sessionData;
        private ISketchCatalog sketchCatalog;

        [InjectionConstructor]
        public SketchSaver(IEventAggregator eventAggregator, SessionData sessionData, ISketchCatalog sketchCatalog)
        {
            this.eventAggregator = eventAggregator;
            this.sessionData = sessionData;
            this.sketchCatalog = sketchCatalog;

            eventAggregator.GetEvent<SaveSketchEvent>().Subscribe(OnSaveSketch);
        }

        private void OnSaveSketch(object dummy)
        {
            // synchronize modelling session changed back to SketchData
            sessionData.SketchData.Cylinders =
                (from cylinderVM in sessionData.NewPrimitives.OfType<NewCylinder>()
                 select new NewCylinder
                 {
                     Axis = cylinderVM.Axis,
                     Center = cylinderVM.Center,
                     Diameter = cylinderVM.Diameter,
                     Length = cylinderVM.Length,
                 }).ToArray();

            // save the new SketchData to the relevant files
            Work.Execute(
                eventAggregator,
                () => sketchCatalog.SaveSketchAsync(sessionData.SketchName, sessionData.SketchData));
        }
    }
}
