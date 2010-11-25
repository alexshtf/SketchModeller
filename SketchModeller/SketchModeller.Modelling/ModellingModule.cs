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
            // register services
            container.RegisterType<ISketchProcessing, SketchProcessing>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISketchCatalog, SketchCatalog>(new ContainerControlledLifetimeManager());
            container.RegisterType<DisplayOptions, DisplayOptions>(new ContainerControlledLifetimeManager());

            // register views.
            regionManager.RegisterViewWithRegion(RegionNames.Sketch, typeof(SketchImageView));
            regionManager.RegisterViewWithRegion(RegionNames.Toolbar, typeof(OpenImageView));
            regionManager.RegisterViewWithRegion(RegionNames.Toolbar, typeof(DisplayOptionsView));
        }
    }
}
