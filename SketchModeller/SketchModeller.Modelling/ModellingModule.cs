﻿using System;
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

            // register global objects objects
            container.RegisterInstance(container.Resolve<SketchLoader>(), new ContainerControlledLifetimeManager());
            container.RegisterInstance(container.Resolve<SketchSaver>(), new ContainerControlledLifetimeManager());

            // register views.
            regionManager.RegisterViewWithRegion(RegionNames.Sketch, typeof(SketchView));
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, typeof(OpenImageView));
            regionManager.RegisterViewWithRegion(RegionNames.MainMenu, typeof(DisplayOptionsView));
            regionManager.RegisterViewWithRegion(RegionNames.ToolBar, typeof(PrimitivesToolbar));
            regionManager.RegisterViewWithRegion(RegionNames.Model, typeof(ModelViewerView));

            var sketchPlanesView = container.Resolve<SketchPlanesView>();
            var sidebar = regionManager.Regions[RegionNames.SideBar];
            sidebar.Add(sketchPlanesView);
            sidebar.Activate(sketchPlanesView);
        }
    }
}
