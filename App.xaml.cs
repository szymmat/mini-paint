using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;

namespace WpfApp6
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
            //Resource1.Culture = new CultureInfo("en-GB");
        }
    }
}
