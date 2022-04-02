using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AZW.FacialOSC.Model;
using AZW.FacialOSC.Properties;
using AZW.FacialOSC.Tracking;
using ViveSR.anipal.Eye;

namespace AZW.FacialOSC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Controller? Controller;
        public MainWindow()
        {
            InitializeComponent();
            //mainPanel.Visibility = Visibility.Hidden;
            var controller = Controller.Instance;

            controller.Init(this).Wait();
        }
        void GainValue_Inputted(object sender, RoutedEventArgs e)
        {
            var ui = sender as TextBox;
            var row = ui.DataContext as SignalRow;
        }

        private void Slider_Gain_ValueChanged(object sender, RoutedEventArgs e)
        {
            var ui = sender as Slider;
            var row = ui.DataContext as SignalRow;
            Controller?.MarkDirty();
        }

        public ObservableCollection<SignalRow> Items { get; private set; }
            = new ObservableCollection<SignalRow>();

        private void facialTrackerButton_Clicked(object sender, RoutedEventArgs args)
        {
            Controller?.SwitchFacialTracker();
        }
        private void Filter_Clicked(object sender, SelectionChangedEventArgs args)
        {
            Controller?.ChangeFilter(sender, args);
        }
        private void eyeTrackerButton_Clicked(object sender, RoutedEventArgs args)
        {
            Controller?.SwitchEyeTracker();
        }
        private void centerButton_Click(object sender, RoutedEventArgs e)
        {
            var ui = (Button)sender;
            var row = (SignalRow)ui.DataContext;

            switch (row.ValueRange)
            {
                case Model.Range.Fixed:
                    return;
                case Model.Range.HalfCentered:
                    row.ValueRange = Model.Range.ZeroCentered;
                    break;
                case Model.Range.ZeroCentered:
                    row.ValueRange = Model.Range.HalfCentered;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Controller?.MarkDirty();
        }

        private void EyeTrackerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ui = (ComboBox) sender;
            var row = (EyeTrackingType) ui.SelectedValue;

        }

        private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ui = (ComboBox)sender;
            var culture = (string)ui.SelectedValue;
            Controller?.LanguageChanged(culture);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Controller?.Save();
        }
    }

    [ValueConversion(typeof(Model.Range), typeof(string))]
    public class CenterToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Model.Range center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case Model.Range.Fixed:
                case Model.Range.HalfCentered:
                    return "0..1";
                case Model.Range.ZeroCentered:
                    return "-1..1";
                default:
                    throw new NotImplementedException(nameof(value));
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not string str) return DependencyProperty.UnsetValue;

            return Enum.Parse<Model.Range>(str);
        }
    }
    public class CenterToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Model.Range center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case Model.Range.Fixed:
                    return Visibility.Hidden;
                case Model.Range.HalfCentered:
                case Model.Range.ZeroCentered:
                    return Visibility.Visible;
                default:
                    throw new NotImplementedException(nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.Range.Fixed : Model.Range.HalfCentered;
        }
    }

    [ValueConversion(typeof(DeviceStatus), typeof(SolidColorBrush))]
    public class DeviceStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not DeviceStatus status) return DependencyProperty.UnsetValue;

            switch (status)
            {
                case DeviceStatus.Unavailable:
                    return new SolidColorBrush() { Color = Color.FromRgb(253, 251, 159) };
                case DeviceStatus.Disbled:
                    return new SolidColorBrush() { Color = Color.FromRgb(238, 62, 91) };
                case DeviceStatus.Starting:
                    return new SolidColorBrush() { Color = Color.FromRgb(160, 165, 250) };
                case DeviceStatus.Running:
                    return new SolidColorBrush() { Color = Color.FromRgb(139, 235, 161) };
                case DeviceStatus.Stopping:
                    return new SolidColorBrush() { Color = Color.FromRgb(160, 165, 250) };
                default:
                    throw new NotImplementedException(nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.Range.Fixed : Model.Range.HalfCentered;
        }
    }

    [ValueConversion(typeof(DeviceStatus), typeof(string))]
    public class DeviceStatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not DeviceStatus status) return DependencyProperty.UnsetValue;

            switch (status)
            {
                case DeviceStatus.Unavailable:
                    return Resources.TrackingUnavailable;
                case DeviceStatus.Disbled:
                    return Resources.TrackingDisabled;
                case DeviceStatus.Starting:
                    return Resources.TrackingStarting;
                case DeviceStatus.Running:
                    return Resources.TrackingRunning;
                case DeviceStatus.Stopping:
                    return Resources.TrackingStopping;
                default:
                    throw new NotImplementedException(nameof(value));
            }
        }
        // TODO
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.Range.Fixed : Model.Range.HalfCentered;
        }
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class DirtyToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not bool status) return DependencyProperty.UnsetValue;

            return status ? Resources.ConfigButtonSaveDirty : Resources.ConfigButtonSave;
        }

        // TODO
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.Range.Fixed : Model.Range.HalfCentered;
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class NegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not bool status) return DependencyProperty.UnsetValue;

            return !status;
        }

        // TODO
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }

    public enum OSCDataFilter
    {
        All, OnlyEnabled, Essential, EyeRaw, EyeComputed, Gaze, LipRaw, LipComputed
    }
}
