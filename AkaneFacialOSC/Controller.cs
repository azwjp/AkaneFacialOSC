using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Azw.FacialOsc.Common;
using Azw.FacialOsc.Common.Model;
using Azw.FacialOsc.Model;
using Azw.FacialOsc.Properties;
using Azw.FacialOsc.Service;
using Azw.FacialOsc.Tracking;
using Azw.FacialOsc.View;
using ViveSR.anipal.Eye;

namespace Azw.FacialOsc
{
    internal class Controller
    {


        static Controller? _instance;
        internal static Controller Instance
        {
            get {
                if (_instance == null) _instance = new Controller();
                return _instance;
            }
        }
        internal MainWindow? mainWindow;

        private LipTracker lip;
        private EyeTracker eye;
        internal TrackingStatus TrackingStatus { get; set; } = new();
        internal Configurations Configs { get; set; } = new();
        public Rows rows = new ();

        private Log log;

        internal IDictionary<FaceKey, SignalProperty> Signals { get { return rows.originalList; } }

        private Task? configLoadingTask = null;

        public Controller()
        {
            log = new(this);
            lip = new(this);
            eye = new(this);
        }

        private async Task LoadAsync(PreferencesV2 preference, bool retry)
        {
            try {
                var ap = preference.applicationPreference;
                var tp = preference.trackingPreference;

                Configs.ApplicationTheme = ap.Theme;
                _ = Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AkaneThemes.Use(ap.Theme); // Need to update the theme manually to set the front brush
                });
                Configs.Language = ap.language;

                TrackingStatus.LipType = tp.LipTracker;
                TrackingStatus.EyeType = tp.EyeTracker;
                TrackingStatus.MaxAngle = tp.maxAngle;
                TrackingStatus.EyeTrackerTargetFps = tp.eyeFps;
                TrackingStatus.LipTrackerTargetFps = tp.lipFps;

                tp.faceDataPreferences.ForEach(p => rows.originalList[p.FaceKey].InitRow(p.FaceKey, p.isSending, p.gain, p.curve, p.isClipping, p.CenterKey));

            }
            catch (Exception ex)
            {
                log.AddLog(Resources.MessageLoadingConfigError, ex);

                if (retry) await LoadAsync(new PreferencesV2(), false);
                else throw;
            }
        }
        
        private async Task LoadAsync()
        {
            var preference = await Task.Run(() =>
            {
                try
                {
                    var pref = PreferencesV2.Load();
                    if (pref.trackingPreference == null) pref.trackingPreference = new PreferencesV2.TrackingPreference();
                    if (pref.applicationPreference == null) pref.applicationPreference = new PreferencesV2.ApplicationPreference();
                    return pref;
                }
                catch (Exception ex)
                {
                    log.AddLog(Resources.MessageLoadingConfigError, ex);
                    return new PreferencesV2();
                }
            }).ConfigureAwait(false);
            configLoadingTask = LoadAsync(preference, true);
            await configLoadingTask;
        }

        internal async Task InitApp()
        {
            Configs.Controller = this;
            TrackingStatus.Controller = this;
            TrackingStatus.DisplayingSignalList = new ObservableCollection<SignalProperty>(Signals.Select(kv => kv.Value).ToList());

            await LoadAsync().ConfigureAwait(false);

            eye.PropertyChanged();
            lip.PropertyChanged();
            eye.CheckingDataHandler += (data, values) => _ = mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                foreach (var o in values.Where(e => FaceKeyUtils.GetTrackingType(e.Key) == TrackingType.Eye))
                {
                    Signals[o.Key].Value = o.Value;
                }
                mainWindow.eyeAppFps.Text = data.ApplicationFps.ToString("0.000");
                mainWindow.eyeDeviceFps.Text = data.TrackingFps.ToString("0.000");
            }
            );
            eye.StatusChangedHandler += (status, detail) =>
            {
                TrackingStatus.EyeTrackingStatus = status;
                switch (status)
                {
                    case DeviceStatus.Starting:
                        if (TrackingStatus.EyeType == EyeTrackingType.ViveSRanipal)
                        {
                            _ = Task.Run(() => {
                                if (!SRanipal_Eye.IsViveProEye())
                                {
                                    log.AddLog(Resources.MessageNotProEye);
                                }
                            }).ConfigureAwait(false);
                            _ = Task.Run(() =>
                            {
                                var isNeedCalibration = false;
                                SRanipal_Eye.IsUserNeedCalibration(ref isNeedCalibration);
                                if (isNeedCalibration)
                                {
                                    log.AddLog(Resources.MessageCalibrationRequired);
                                }
                            }).ConfigureAwait(false);
                        }
                        break;
                    case DeviceStatus.Disbled:
                        mainWindow?.Dispatcher.InvokeAsync(() =>
                        {
                            mainWindow.eyeAppFps.Text = 0d.ToString("0.000");
                            mainWindow.eyeDeviceFps.Text = 0d.ToString("0.000");
                        });
                        break;
                    case DeviceStatus.Unavailable:
                        switch (eye.TrackingDevice)
                        {
                            case EyeTrackingType.ViveSRanipal:
                                log.AddLog(Resources.MessageDeviceError, ((ViveSR.Error)detail).ToString());
                                break;
                            default:
                                break;
                        }
                        break;
                }
            };
            lip.CheckingDataHandler += (data, values) => _ = mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                foreach (var o in values.Where(e => FaceKeyUtils.GetTrackingType(e.Key) == TrackingType.Lip))
                {
                    Signals[o.Key].Value = o.Value;
                }
                mainWindow.lipAppFps.Text = data.ApplicationFps.ToString("0.000");
                mainWindow.lipDeviceFps.Text = data.TrackingFps.ToString("0.000");
                }
            );
            lip.StatusChangedHandler += (status, detail) =>
            {
                TrackingStatus.LipTrackingStatus = status;
            };

            Configs.IsDirty = false;
        }

        internal async Task PreInitWindowAsync()
        {
            if (configLoadingTask == null) configLoadingTask = LoadAsync();
            if (configLoadingTask != null && !configLoadingTask.IsCompleted) configLoadingTask.Wait();

            var version = await VersionCheck.CheckAsync().ConfigureAwait(false);
            if (version == null)
            {
                log.AddLog(Resources.MessageUpdateFailed);
            }
            else if (version.UpdateExist())
            {
                log.AddLog(Resources.MessageUpdateExists, Resources.MessageCurrentVersion, version.Current, Resources.MessageLatestVersion, version.Latest);
            }
            else if (version.IsLatestDifferent())
            {
                log.AddLog(Resources.MessageDifferentVersionExists, Resources.MessageCurrentVersion, version.Current, Resources.MessageLatestVersion, version.Latest);
            }
        }

        internal async Task SwitchEyeTracker()
        {
            Task.Run(() => eye.Switch()).ConfigureAwait(false);
        }


        internal async Task SwitchFacialTracker()
        {
            Task.Run(() => lip.Switch()).ConfigureAwait(false);
        }

        internal async Task ChangeEyeTracker(EyeTrackingType trackingType)
        {
            if (trackingType == eye.TrackingDevice) return;

            eye.TrackingDevice = trackingType;
            Task.Run(() => eye.Stop()).ConfigureAwait(false);
        }

        internal async Task ChangeLipTracker(LipTrackingType trackingType)
        {
            if (trackingType == lip.TrackingDevice) return;

            lip.TrackingDevice = trackingType;
            Task.Run(() => lip.Stop()).ConfigureAwait(false);
        }

        bool isModifyingFilter = false;

        internal void ChangeFilter(object sender, SelectionChangedEventArgs args)
        {
            if (isModifyingFilter) return;

            var ui = (ListBox)sender;
            var selected = ui.SelectedItems;
            var selectedKeys = ui.SelectedItems
                .Cast<KeyValuePair<OSCSignalFilter, string>>()
                .Select(kv => kv.Key).ToList();
            var addedKeys = args.AddedItems
                .Cast<KeyValuePair<OSCSignalFilter, string>>()
                .Select(kv => kv.Key).ToList();

            isModifyingFilter = true;
            if (addedKeys.Count == 0 && selectedKeys.Count == 0)
            {
                selected.Clear();
                selected.Add(new KeyValuePair<OSCSignalFilter, string>(OSCSignalFilter.All, Resources.OSCSignalFilterAll));
            }
            else if (addedKeys.Contains(OSCSignalFilter.All))
            {
                selected.Clear();
                selected.Add(new KeyValuePair<OSCSignalFilter, string>(OSCSignalFilter.All, Resources.OSCSignalFilterAll));
            }
            else if (addedKeys.Contains(OSCSignalFilter.OnlyEnabled))
            {
                selected.Clear();
                selected.Add(new KeyValuePair<OSCSignalFilter, string>(OSCSignalFilter.OnlyEnabled, Resources.OSCSignalFilterOnlyEnabled));
            }
            else
            {
                if (selectedKeys.Contains(OSCSignalFilter.All))
                {
                    selected.RemoveAt(selectedKeys.IndexOf(OSCSignalFilter.All));
                }

                if (selectedKeys.Contains(OSCSignalFilter.OnlyEnabled))
                {
                    selected.RemoveAt(selectedKeys.IndexOf(OSCSignalFilter.OnlyEnabled));
                }

            }
            isModifyingFilter = false;

            _ = Task.Run(() =>
            {
                var toBeDisplayed = selected
                    .Cast<KeyValuePair<OSCSignalFilter, string>>()
                    .Select(kv => kv.Key)
                    .SelectMany(filter =>
                        Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().Where(k =>
                            filter switch
                            {
                                OSCSignalFilter.All => true,
                                OSCSignalFilter.OnlyEnabled => Signals[k].IsSending,
                                OSCSignalFilter.Essential => FaceKeyUtils.IsEssential(k),
                                OSCSignalFilter.EyeRaw => FaceKeyUtils.GetDataType(k) == DataType.Eye,
                                OSCSignalFilter.EyeComputed => FaceKeyUtils.GetDataType(k) == DataType.ComputedEye,
                                OSCSignalFilter.Gaze => FaceKeyUtils.GetDataType(k) == DataType.Gaze,
                                OSCSignalFilter.LipRaw => FaceKeyUtils.GetDataType(k) == DataType.Facial,
                                OSCSignalFilter.LipComputed => FaceKeyUtils.GetDataType(k) == DataType.ComputedFacial,
                                _ => throw new UnexpectedEnumValueException(filter),
                            })
                    )
                    .Distinct()
                    .Select(k => Signals[k]);

                TrackingStatus.DisplayingSignalList = new ObservableCollection<SignalProperty>(toBeDisplayed);

                mainWindow?.Dispatcher.Invoke(() =>
                {
                    mainWindow?.CheckCheckedAll();
                });
            }).ConfigureAwait(false);
        }

        internal void BulkChange(bool toBe)
        {
            var isDirty = false;
            foreach (var signal in TrackingStatus.DisplayingSignalList)
            {
                if (signal.IsSending != toBe)
                {
                    isDirty = true;
                    signal.IsSending = toBe;
                }
            }

            if (isDirty) MarkDirty();
        }

        internal void SetEyeFps(double fps)
        {
            mainWindow?.Dispatcher.InvokeAsync(() => mainWindow.eyeTargetFps.Text = fps.ToString("0.000"));
            TrackingStatus.EyeTrackerTargetFps = fps;
            eye.PropertyChanged();
        }
        internal void SetLipFps(double fps)
        {
            mainWindow?.Dispatcher.InvokeAsync(() => mainWindow.lipTargetFps.Text = fps.ToString("0.000"));
            TrackingStatus.LipTrackerTargetFps = fps;
            lip.PropertyChanged();
        }

        internal Task RevertConfigs()
        {
            return InitApp();
        }
        internal Task ResetAll()
        {
            return LoadAsync(new PreferencesV2(), false);
        }

        internal void Save()
        {
            Task.Run(() =>
            {
                var trackingPref = TrackingStatus.ToPreference(Signals.Values);

                var appPref = Configs.ToPreference();

                var pref = new PreferencesV2()
                {
                    trackingPreference = trackingPref,
                    applicationPreference = appPref,
                }.Save();

                Configs.IsDirty = false;
            }).ConfigureAwait(false);
        }

        internal void LanguageChanged(string cultureName)
        {
            Task.Run(() =>
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                ResourceService.Current.ChangeCulture(culture);
                Configs.Language = cultureName;
                TrackingStatus.NotifyPropertyChanged(nameof(TrackingStatus.EyeTrackingStatus));
                TrackingStatus.NotifyPropertyChanged(nameof(TrackingStatus.LipTrackingStatus));
                TrackingStatus.NotifyPropertyChanged(nameof(TrackingStatus.FilterList));
                Configs.NotifyPropertyChanged(nameof(Configs.IsDirty));
            }).ConfigureAwait(false);
        }

        internal void MarkDirty()
        {
            Configs.IsDirty = true;
            eye.PropertyChanged();
            lip.PropertyChanged();
        }

        internal void DisposeAll()
        {
            Task.WaitAll(
                Task.Run(() => eye.Dispose()),
                Task.Run(() => lip.Dispose())
            );
        }

        internal void UnhandledException(string message, Exception exception)
        {
            log.UnhandledException(message, exception);
        }
    }
}
