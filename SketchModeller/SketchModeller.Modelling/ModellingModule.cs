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
            // register shared data
            container.RegisterType<DisplayOptions, DisplayOptions>(new ContainerControlledLifetimeManager());
            container.RegisterType<SessionData, SessionData>(new ContainerControlledLifetimeManager());
            container.RegisterType<UiState, UiState>(new ContainerControlledLifetimeManager());

            // register services
            container.RegisterType<ISketchProcessing, SketchProcessing>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISketchCatalog, SketchCatalog>(new ContainerControlledLifetimeManager());

            // register global objects objects
            container.RegisterInstance(container.Resolve<SketchLoader>(), new ContainerControlledLifetimeManager());

            // register views.
            regionManager.RegisterViewWithRegion(RegionNames.Sketch, typeof(SketchImageView));
            regionManager.RegisterViewWithRegion(RegionNames.Sketch, typeof(SketchModellingView));
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, typeof(OpenImageView));
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, typeof(DisplayOptionsView));
            regionManager.RegisterViewWithRegion(RegionNames.ToolBar, typeof(PrimitivesToolbar));
        }
    }
}
