using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Azw.FacialOsc.Model;
using MahApps.Metro.Controls;

namespace Azw.FacialOsc
{
    /// <summary>
    /// AboutWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutWindow : MetroWindow
    {
        public static AboutWindow? Instance = null;
        public AboutWindow()
        {
            InitializeComponent();
            versionLabel.Text = VersionCheck.CurrentVersion();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
            e.Handled = true;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Instance = null;
        }
    }
}
