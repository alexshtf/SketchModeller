using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Windows;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Prism.Modularity;
using SketchModeller.Modelling;

namespace SketchModeller
{
    class SketchModellerBootstrapper : UnityBootstrapper
    {
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
