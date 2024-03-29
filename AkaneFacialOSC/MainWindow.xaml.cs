﻿using System;
using System.Linq;
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
using System.Collections.Generic;
using Azw.FacialOsc.Common;

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

            FilterList.SelectedIndex = 0;

            CheckCheckedAll();
            mainPanel.Visibility = Visibility.Visible;
        }

        internal void CheckCheckedAll()
        {
            var checkedCount = Controller.TrackingStatus.DisplayingSignalList.Count(s => s.IsSending);
            if (checkedCount == 0)
            {
                SelectAll.IsChecked = false;
            }
            else if (checkedCount == Controller.TrackingStatus.DisplayingSignalList.Count)
            {
                SelectAll.IsChecked = true;
            }
            else
            {
                SelectAll.IsChecked = null;
            }
        }

        private void MarkDirty(object sender, RoutedEventArgs e)
        {
            Controller?.MarkDirty();
        }

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
                case ValueRange.ZeroToOne:
                    row.ValueRange = ValueRange.MinusOneToOne;
                    break;
                case ValueRange.MinusOneToOne:
                    row.ValueRange = ValueRange.ZeroToOne;
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

        private void BulkCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            var ui = (CheckBox)sender;
            Controller?.BulkChange(ui.IsChecked != false);
        }
        private void IsSendingCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            Controller?.MarkDirty();
            CheckCheckedAll();
        }
    }

    [ValueConversion(typeof(ValueRange), typeof(string))]
    public class CenterToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not ValueRange center) return DependencyProperty.UnsetValue;

            switch (center)
            {
                case ValueRange.ZeroToOne:
                    return "0..1";
                case ValueRange.MinusOneToOne:
                    return "-1..1";
                default:
                    throw new UnexpectedEnumValueException(center);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not string str) return DependencyProperty.UnsetValue;

            return Enum.Parse<ValueRange>(str);
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

            return null;
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

            return null;
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

            return null;
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

    [ValueConversion(typeof(IDictionary<OSCSignalFilter, string>), typeof(string))]
    public class OscFiltgerToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is not IDictionary<OSCSignalFilter, string> statuses) return DependencyProperty.UnsetValue;

            return statuses.ToDictionary(s => s.Key, s => s.Key switch
            {
                OSCSignalFilter.All => Resources.OSCSignalFilterAll,
                OSCSignalFilter.OnlyEnabled => Resources.OSCSignalFilterOnlyEnabled,
                OSCSignalFilter.Essential => Resources.OSCSignalFilterEssential,
                OSCSignalFilter.Gaze => Resources.OSCSignalFilterGaze,
                OSCSignalFilter.EyeRaw => Resources.OSCSignalFilterEyeRaw,
                OSCSignalFilter.EyeComputed => Resources.OSCSignalFilterEyeComputed,
                OSCSignalFilter.LipRaw => Resources.OSCSignalFilterLipRaw,
                OSCSignalFilter.LipComputed => Resources.OSCSignalFilterLipComputed,
                _ => s.ToString(),
            });
        }

        // TODO
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }
    public enum OSCSignalFilter
    {
        All, OnlyEnabled, Essential, Gaze, EyeRaw, EyeComputed, LipRaw, LipComputed
    }
}
