using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using SketchModeller.Infrastructure;
using SketchModeller.Modelling.Views;
using SketchModeller.Infrastructure.Services;
using SketchModeller.Modelling.Services.Sketch;
using SketchModeller.Infrastructure.Shared;
using System.Windows.Markup;
using System.Windows;
using SketchModeller.Modelling.ModelViews;
using SketchModeller.Modelling.Services.Snap;
using SketchModeller.Modelling.Services.PrimitivesConverter;
using SketchModeller.Modelling.Services.Assign;
using SketchModeller.Modelling.Services.AnnotationInference;

namespace SketchModeller.Modelling
{
    public class ModellingModule : IModule
    {
        private IUnityContainer container;
        private IRegionManager regionManager;

        public ModellingModule(IUnityContainer container)
        {
            this.container = container;
            regionManager = container.Resolve<IRegionManager>();
        }

        public void Initialize()
        {
            // register services
            container.RegisterType<ISketchCatalog, SketchCatalog>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAnnotationInference, AnnotationInferenceService>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISnapper, Snapper>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPrimitivesConverter, PrimitivesConverter>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICurveAssigner, CurveAssigner>(new ContainerControlledLifetimeManager());

            // register global objects objects
            container.RegisterInstance(container.Resolve<SketchLoader>(), new ContainerControlledLifetimeManager());
            container.RegisterInstance(container.Resolve<SketchSaver>(), new ContainerControlledLifetimeManager());
            container.RegisterInstance(container.Resolve<TestCaseCreator>(), new ContainerControlledLifetimeManager());

            // register views.
            regionManager.RegisterViewWithRegion(RegionNames.Sketch, typeof(SketchView));
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, typeof(OpenImageView));
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, typeof(DisplayOptionsView));
            regionManager.RegisterViewWithRegion(RegionNames.Model, typeof(ModelViewerView));

            var sketchPlanesView = container.Resolve<SketchPlanesView>();
            var annotationsView = container.Resolve<AnnotationsView>();
            var sidebar = regionManager.Regions[RegionNames.SideBar];
            sidebar.Add(annotationsView);
            sidebar.Add(sketchPlanesView);
            sidebar.Activate(annotationsView);
        }
    }
}
