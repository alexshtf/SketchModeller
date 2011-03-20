using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using WPF.Themes;

namespace SketchModeller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //var themes = ThemeManager.GetThemes();
            ThemeManager.ApplyTheme(this, "ExpressionLight");

            var bootstrapper = new SketchModellerBootstrapper();
            bootstrapper.Run();
        }
    }
}
