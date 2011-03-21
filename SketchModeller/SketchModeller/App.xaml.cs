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

            ThemeManager.ApplyTheme(this, "ExpressionDark");
            //ThemeManager.ApplyTheme(this, "ExpressionLight");
            //ThemeManager.ApplyTheme(this, "ShinyBlue");
            //ThemeManager.ApplyTheme(this, "ShinyRed");
            //ThemeManager.ApplyTheme(this, "DavesGlossyControls");
            //ThemeManager.ApplyTheme(this, "WhistlerBlue");
            //ThemeManager.ApplyTheme(this, "BureauBlack");
            //ThemeManager.ApplyTheme(this, "BureauBlue");
            //ThemeManager.ApplyTheme(this, "BubbleCreme");
            //ThemeManager.ApplyTheme(this, "TwilightBlue");
            //ThemeManager.ApplyTheme(this, "UXMusingsRed");
            //ThemeManager.ApplyTheme(this, "UXMusingsGreen");
            //ThemeManager.ApplyTheme(this, "UXMusingsBubblyBlue");

            var bootstrapper = new SketchModellerBootstrapper();
            bootstrapper.Run();
        }
    }
}
