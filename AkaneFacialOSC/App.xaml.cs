using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Azw.FacialOsc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _ = Controller.Instance.InitApp();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception.InnerException ?? e.Exception);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            HandleException(exception);
            MessageBox.Show($"{FacialOsc.Properties.Resources.MessageUnexpectedError} {exception.Message}", "Error");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Controller.Instance.DisposeAll();
        }
        private void HandleException(Exception e)
        {
            Controller.Instance.UnhandledException(FacialOsc.Properties.Resources.MessageUnexpectedError, e);
        }
    }
}
