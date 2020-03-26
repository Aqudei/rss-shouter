using System;
using System.IO;
using System.Windows;
using NLog;

namespace RSSLoudReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;

//#if DEBUG

//#else
//            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RssLoudShouter");        
//            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
//#endif
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception, e.Exception.Message);
        }
    }
}
