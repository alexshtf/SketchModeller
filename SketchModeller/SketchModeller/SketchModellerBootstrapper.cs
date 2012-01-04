using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Windows;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Prism.Modularity;
using SketchModeller.Modelling;
using Microsoft.Practices.Prism.Regions;
using Controls;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using SketchModeller.Infrastructure.Shared;
using Microsoft.Practices.Prism.Logging;

namespace SketchModeller
{
    class SketchModellerBootstrapper : UnityBootstrapper
    {
        protected override ILoggerFacade CreateLogger()
        {
            return new TraceLogger();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            // register shared data
            Container.RegisterType<DisplayOptions, DisplayOptions>(new ContainerControlledLifetimeManager());
            Container.RegisterType<SessionData, SessionData>(new ContainerControlledLifetimeManager());
            Container.RegisterType<UiState, UiState>(new ContainerControlledLifetimeManager());

        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mapping = base.ConfigureRegionAdapterMappings();
            mapping.RegisterMapping(typeof(ModelVisual3D), Container.Resolve<ModelVisual3DRegionAdapter>());
            mapping.RegisterMapping(typeof(ToolBarTray), Container.Resolve<ToolBarTrayRegionAdapter>());
            return mapping;
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var moduleCatalog = base.CreateModuleCatalog();

            var modellingModule = typeof(ModellingModule);
            moduleCatalog.AddModule(new ModuleInfo
            {
                ModuleName = modellingModule.Name,
                ModuleType = modellingModule.AssemblyQualifiedName,
                InitializationMode = InitializationMode.WhenAvailable,
            });

            return moduleCatalog;
        }

        protected override void InitializeModules()
        {
            base.InitializeModules();
        }
    }
}
