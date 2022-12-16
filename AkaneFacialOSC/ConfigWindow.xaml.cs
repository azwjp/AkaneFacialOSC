using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Azw.FacialOsc
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : MetroWindow
    {
        public static ConfigWindow? Instance;
        internal Controller Controller;
        public ConfigWindow()
        {
            InitializeComponent();

            Controller = Controller.Instance;

            configs.DataContext = Controller.Configs;
            trackingConfigs.DataContext = Controller.TrackingStatus;
            eyeType.DataContext = Controller.TrackingStatus;
            lipType.DataContext = Controller.TrackingStatus;
            eyeType.SelectedValue = Controller.TrackingStatus.EyeType;
            lipType.SelectedValue = Controller.TrackingStatus.LipType;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Instance = null;
        }

        private void RevertButton_Click(object sender, RoutedEventArgs e)
        {
            var metroDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = Properties.Resources.ButtonOK,
                NegativeButtonText = Properties.Resources.ButtonCancel,
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme,
            };

            _ = Task.Run(async () =>
            {
                var diagResult = await Dispatcher.Invoke(() => this.ShowMessageAsync(Properties.Resources.ButtonRevert, Properties.Resources.DialogRevertMessage, MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings));
                if (diagResult == MessageDialogResult.Affirmative)
                {
                    Controller?.RevertConfigs();
                }
            }).ConfigureAwait(false);
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var metroDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = Properties.Resources.ButtonOK,
                NegativeButtonText = Properties.Resources.ButtonCancel,
                AnimateHide = true,
                AnimateShow = true,
                ColorScheme = MetroDialogColorScheme.Theme,
            };

            _ = Task.Run(async () =>
            {
                var diagResult = await Dispatcher.Invoke(() => this.ShowMessageAsync(Properties.Resources.ButtonResetAll, Properties.Resources.DialogResetAllMessage, MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings));
                if (diagResult == MessageDialogResult.Affirmative)
                {
                    Controller?.ResetAll();
                }
            }).ConfigureAwait(false);
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void EyeTrackerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ui = (ComboBox)sender;
            var trackingType = (EyeTrackingType)ui.SelectedValue;
            Controller?.ChangeEyeTracker(trackingType);
        }
        private void LipTrackerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ui = (ComboBox)sender;
            var trackingType = (LipTrackingType)ui.SelectedValue;
            Controller?.ChangeLipTracker(trackingType);
        }
    }
}
