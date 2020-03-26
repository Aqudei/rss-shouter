using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using RSSLoudReader.ViewModels;
using Unity;
using LogManager = NLog.LogManager;

namespace RSSLoudReader
{
    class Bootstrapper : BootstrapperBase
    {
        private readonly IUnityContainer _container = new UnityContainer();
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        public Bootstrapper()
        {
            _logger.Info("Bootstrapper Initializing...");
            Initialize();
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void Configure()
        {
            _container.RegisterSingleton<IEventAggregator, EventAggregator>();
            _container.RegisterType<IWindowManager, WindowManager>();
            _container.RegisterInstance(DialogCoordinator.Instance);
            base.Configure();
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.ResolveAll(service);
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.Resolve(service, key);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
