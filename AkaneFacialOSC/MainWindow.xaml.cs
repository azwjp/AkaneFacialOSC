using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Azw.FacialOsc.Model;
using Azw.FacialOsc.Properties;
using Azw.FacialOsc.Tracking;
using Azw.FacialOsc.View;
using MahApps.Metro.Controls;

namespace Azw.FacialOsc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        internal Controller Controller = Controller.Instance;
        public MainWindow()
        {
            InitializeComponent();
            mainPanel.Visibility = Visibility.Hidden;

            Controller = Controller.Instance;
            Controller.mainWindow = this;
            _ = Controller.PreInitWindowAsync();

            DataContext = Controller.TrackingStatus;
            dirtyButton.DataContext = Controller.Configs;
            facialTrackerButton.DataContext = Controller.TrackingStatus;
            eyeTrackerButton.DataContext = Controller.TrackingStatus;

            FilterList.SelectedItem = OSCDataFilter.All;
            FilterList.SelectedItems.Clear();
            FilterList.SelectedItems.Add(OSCDataFilter.All);
            mainPanel.Visibility = Visibility.Visible;
        }
        void GainValue_Inputted(object sender, RoutedEventArgs e)
        {
            var ui = sender as TextBox;
            var row = ui.DataContext as SignalProperty;
        }

        private void Slider_Gain_ValueChanged(object sender, RoutedEventArgs e)
        {
            var ui = sender as Slider;
            var row = ui.DataContext as SignalProperty;
            Controller?.MarkDirty();
        }

        public ObservableCollection<SignalProperty> Items { get; private set; }
            = new ObservableCollection<SignalProperty>();

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
            var row = (SignalProperty)ui.DataContext;

            switch (row.ValueRange)
            {
                case Model.ValueRange.Fixed:
                    return;
                case Model.ValueRange.HalfCentered:
                    row.ValueRange = Model.ValueRange.ZeroCentered;
                    break;
                case Model.ValueRange.ZeroCentered:
                    row.ValueRange = Model.ValueRange.HalfCentered;
                    break;
                default:
                    throw new UnexpectedEnumValueException(row.ValueRange);
            }
            Controller?.MarkDirty();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Controller?.Save();
        }
        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigWindow.Instance != null) return;

            var w = new ConfigWindow();
            w.Owner = this;
            w.Left = Left + Width / 2 - w.Width / 2;
            w.Top = Top + Height / 2 - w.Height / 2;
            ConfigWindow.Instance = w;
            w.Show();
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            if (AboutWindow.Instance != null) return;

            var w = new AboutWindow();
            w.Owner = this;
            w.Left = Left + Width / 2 - w.Width / 2;
            w.Top = Top + Height / 2 - w.Height / 2;
            AboutWindow.Instance = w;
            w.Show();
        }

        private void BulkCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Controller?.BulkCheck();
        }
        private void BulkCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            Controller?.BulkUnCheck();
        }
    }

    [ValueConversion(typeof(Model.ValueRange), typeof(string))]
    public class CenterToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Model.ValueRange center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case Model.ValueRange.Fixed:
                case Model.ValueRange.HalfCentered:
                    return "0..1";
                case Model.ValueRange.ZeroCentered:
                    return "-1..1";
                default:
                    throw new UnexpectedEnumValueException(center);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not string str) return DependencyProperty.UnsetValue;

            return Enum.Parse<Model.ValueRange>(str);
        }
    }
    public class CenterToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Model.ValueRange center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case ValueRange.Fixed:
                    return Visibility.Hidden;
                case ValueRange.HalfCentered:
                case ValueRange.ZeroCentered:
                    return Visibility.Visible;
                default:
                    throw new UnexpectedEnumValueException(center);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.ValueRange.Fixed : Model.ValueRange.HalfCentered;
        }
    }

    [ValueConversion(typeof(DeviceStatus), typeof(SolidColorBrush))]
    public class DeviceStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not DeviceStatus status) return DependencyProperty.UnsetValue;

            return status switch
            {
                DeviceStatus.Unavailable => AkaneColors.UnavailableBrush,
                DeviceStatus.Disbled => AkaneColors.DisbledBrush,
                DeviceStatus.Starting => AkaneColors.StartingBrush,
                DeviceStatus.Running => AkaneColors.RunningBrush,
                DeviceStatus.Stopping => AkaneColors.StoppingBrush,
                _ => throw new UnexpectedEnumValueException(status),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.ValueRange.Fixed : Model.ValueRange.HalfCentered;
        }
    }

    [ValueConversion(typeof(DeviceStatus), typeof(string))]
    public class DeviceStatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not DeviceStatus status) return DependencyProperty.UnsetValue;

            return status switch
            {
                DeviceStatus.Unavailable => Resources.TrackingUnavailable,
                DeviceStatus.Disbled => Resources.TrackingDisabled,
                DeviceStatus.Starting => Resources.TrackingStarting,
                DeviceStatus.Running => Resources.TrackingRunning,
                DeviceStatus.Stopping => Resources.TrackingStopping,
                _ => throw new UnexpectedEnumValueException(status),
            };
        }
        // TODO
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;

            return v == Visibility.Collapsed ? Model.ValueRange.Fixed : Model.ValueRange.HalfCentered;
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

            return v == Visibility.Collapsed ? Model.ValueRange.Fixed : Model.ValueRange.HalfCentered;
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
