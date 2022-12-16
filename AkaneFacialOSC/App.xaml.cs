using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
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
        const string AppName = "AZW_AkaneFacialOSC";
        Mutex mutex = new Mutex(false, AppName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern int IsIconic(IntPtr hwnd);


        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!mutex.WaitOne(0, false))
            {
                var others = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
                if (others.Length > 0) {
                    var window = others[0].MainWindowHandle;
                    if (IsIconic(window) != 0) ShowWindow(window, 1); // SW_SHOWNORMAL 
                    SetForegroundWindow(window);
                }
                Shutdown();
                return;
            }

            Controller.Instance.InitApp();

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            base.OnStartup(e);
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
