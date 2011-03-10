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
            // load styles
            var stylesDictionary = new ResourceDictionary { Source = new Uri("/SketchModeller.Modelling;component/styles.xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(stylesDictionary);

            // register services
            container.RegisterType<ISketchCatalog, SketchCatalog>(new ContainerControlledLifetimeManager());
            //container.RegisterType<ISnapper, Snapper>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISnapper, Snapper>(new ContainerControlledLifetimeManager());

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
            var snappedPanelView = container.Resolve<SnappedPanelView>();
            var sidebar = regionManager.Regions[RegionNames.SideBar];
            sidebar.Add(sketchPlanesView);
            sidebar.Add(annotationsView);
            sidebar.Add(snappedPanelView);
            sidebar.Activate(sketchPlanesView);
        }
    }
}
