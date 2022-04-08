using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Azw.FacialOsc.View;
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

        private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ui = (ComboBox)sender;
            var culture = (string)ui.SelectedValue;
            Controller?.LanguageChanged(culture);
        }
        private void ApplicationTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ui = (ComboBox)sender;
            var theme = (AkaneThemes.Themes)ui.SelectedValue;
        }

        private readonly Regex numberRegex = new (@"\d*");
        private bool ValidateNumber(string text) { return numberRegex.IsMatch(text); }
        private void TextBox_ValidateNumber(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidateNumber(e.Text);
        }
        private void TextBox_ValidateNumber_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (ValidateNumber(text)) return;
            }
            e.CancelCommand();
        }
    }
}
