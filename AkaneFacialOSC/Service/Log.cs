using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Azw.FacialOsc.Properties;

namespace Azw.FacialOsc.Service
{
    internal class Log
    {
        private Controller controller;

        private ConcurrentQueue<string> logs = new();

        private const nint LogLength = 100;
        const string ErrorLogDir = "ErrorLogs";


        public Log(Controller controller)
        {
            this.controller = controller;
        }

        internal bool AddLog(string message, Exception ex)
        {
            WriteLog(ex);
            return AddLog(message, Resources.MessageErrorDetail, ex.Message);
        }

        internal bool AddLog(params string?[] texts)
        {
            var mainWindow = controller?.mainWindow;
            var message = string.Join(" ", texts.Where(s => s != null));
            var parent = mainWindow?.logArea;
            var now = DateTime.Now.ToString("T");
            logs.Enqueue(message);

            if (mainWindow != null && parent != null)
            {
                _ = mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    var children = parent.Children;
                    if (LogLength < children.Count) children.RemoveAt(0);

                    string? toBeWritten;
                    while(logs.TryDequeue(out toBeWritten)){
                        if (toBeWritten == null) break;
                        var adding = new TextBlock()
                        {
                            Text = $"[{now}] {toBeWritten}",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 5),
                        };
                        children.Add(adding);
                    }
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        private void WriteLog(Exception exception)
        {
            try
            {
                var dir = Path.Join(Directory.GetCurrentDirectory(), ErrorLogDir);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var path = Path.Combine(dir, $"error_{DateTime.Now.ToString("yyyy-MM-dd")}.log");
                File.AppendAllLines(path, new string[] {
                    DateTime.Now.ToString(),
                    exception.ToString(),
                });
            }
            catch { }
        }

        internal void UnhandledException(string message, Exception exception)
        {
            Debug.WriteLine(exception);

            if (!AddLog(message, exception))
            {
                MessageBox.Show(message + exception.Message);
            }
        }
    }
}
