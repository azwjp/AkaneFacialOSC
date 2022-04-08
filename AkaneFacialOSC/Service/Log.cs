using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Log(Controller controller)
        {
            this.controller = controller;
        }

        internal void AddLog(string message, Exception ex)
        {
            AddLog(message, Resources.MessageErrorDetail, ex.Message);
        }

        internal void AddLog(params string?[] texts)
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
            }
        }
    }
}
