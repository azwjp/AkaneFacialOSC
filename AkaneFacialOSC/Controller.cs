using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private LipTracker? lip;
        private EyeTracker? eye;
        internal TrackingStatus TrackingStatus { get; set; } = new();
        internal Configurations Configs { get; set; } = new();
        public Rows rows = new ();

        OSCService osc = new();
        private Log log;

        internal IDictionary<FaceKey, SignalProperty> Signals { get { return rows.originalList; } }

        private Task<(EyeTracker eye, LipTracker lip)>? configLoadingTask = null;

        public Controller()
        {
            log = new(this);
        }

        private async Task<(EyeTracker eye, LipTracker lip)> LoadAsync(PreferencesV2 preference)
        {
            try { 
                var ap = preference.applicationPreference;
                var tp = preference.trackingPreference;


                _ = Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    AkaneThemes.Use(ap.Theme);
                });
                TrackingStatus.LipType = tp.LipTracker;
                TrackingStatus.EyeType = tp.EyeTracker;
                TrackingStatus.MaxAngle = tp.maxAngle;
                TrackingStatus.EyeTrackerTargetFps = tp.eyeFps;
                TrackingStatus.LipTrackerTargetFps = tp.lipFps;

                tp.faceDataPreferences.ForEach(p => rows.originalList[p.FaceKey].InitRow(p.FaceKey, p.isSending, p.gain, p.curve, p.isClipping, p.CenterKey));


                var eyeChangingTask = ChangeEyeTracker(tp.EyeTracker);
                var lipChangingTask = ChangeLipTracker(tp.LipTracker);
                var eye = await eyeChangingTask;
                var lip = await lipChangingTask;

                return (eye, lip);
            }
            catch (Exception ex)
            {
                log.AddLog(Resources.MessageLoadingConfigError, ex);

                preference = new PreferencesV2();

                var ap = preference.applicationPreference;
                var tp = preference.trackingPreference;

                _ = Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    AkaneThemes.Use(ap.Theme);
                });
                TrackingStatus.LipType = tp.LipTracker;
                TrackingStatus.EyeType = tp.EyeTracker;
                TrackingStatus.MaxAngle = tp.maxAngle;
                TrackingStatus.EyeTrackerTargetFps = tp.eyeFps;
                TrackingStatus.LipTrackerTargetFps = tp.lipFps;

                tp.faceDataPreferences.ForEach(p => rows.originalList[p.FaceKey].InitRow(p.FaceKey, p.isSending, p.gain, p.curve, p.isClipping, p.CenterKey));


                var eyeChangingTask = ChangeEyeTracker(tp.EyeTracker);
                var lipChangingTask = ChangeLipTracker(tp.LipTracker);
                var eye = await eyeChangingTask;
                var lip = await lipChangingTask;

                return (eye, lip);
            }
        }
        
        private async Task<(EyeTracker eye, LipTracker lip)> LoadAsync()
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
            configLoadingTask = LoadAsync(preference);
            return await configLoadingTask;
        }

        internal async Task InitApp()
        {
            Configs.Controller = this;
            TrackingStatus.Controller = this;
            TrackingStatus.DisplayingSignalList = new ObservableCollection<SignalProperty>(Signals.Select(kv => kv.Value).ToList());

            var (eye, lip) = await LoadAsync().ConfigureAwait(false);

            eye.SetTargetFps(TrackingStatus.EyeTrackerTargetFps);
            eye.updatedHandler += (instance, rawData) => TrackerOnUpdated(instance, rawData, TrackingType.Eye);
            eye.checkedHandler += instance => _ = mainWindow?.Dispatcher.InvokeAsync(() =>
            {
                mainWindow.eyeTargetFps.Text = (1 / instance.targetInterval.TotalSeconds).ToString("0.000");
                mainWindow.eyeAppFps.Text = instance.ApplicationFps.averageFps.ToString("0.000");
                mainWindow.eyeDeviceFps.Text = instance.TrackingFps.averageFps.ToString("0.000");
            }
            );
            eye.statusChangedHandler += (instance, status) =>
            {
                TrackingStatus.EyeTrackingStatus = status;

                if (status == DeviceStatus.Unavailable)
                {
                    switch (instance)
                    {
                        case SRanipalEyeTracker s:
                            log.AddLog(Resources.MessageDeviceError, s.deviceStatus.ToString());
                            break;
                        default:
                            break;
                    }
                }
            };
            lip.SetTargetFps(TrackingStatus.LipTrackerTargetFps);
            lip.updatedHandler += (instance, rawData) => TrackerOnUpdated(instance, rawData, TrackingType.Lip);
            lip.checkedHandler += instance => _ = mainWindow?.Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.lipTargetFps.Text = (1 / instance.targetInterval.TotalSeconds).ToString("0.000");
                    mainWindow.lipAppFps.Text = instance.ApplicationFps.averageFps.ToString("0.000");
                    mainWindow.lipDeviceFps.Text = instance.TrackingFps.averageFps.ToString("0.000");
                }
            );
            lip.statusChangedHandler += (instance, status) =>
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
            if (eye == null) eye = await EyeTracker.Instance<SRanipalEyeTracker>().ConfigureAwait(false);

            await eye.Switch().ConfigureAwait(false);
        }

        internal async Task SwitchFacialTracker()
        {
            if (lip == null) lip = await LipTracker.Instance<SRanipalLipTracker>().ConfigureAwait(false);

            var status = await lip.Switch().ConfigureAwait(false);

            // When it is connected with the wireless kit, it could run in the next attempt even if the device throw an error in the first time
            if (status == DeviceStatus.Unavailable && lip.GetType() == typeof(SRanipalLipTracker) && ((SRanipalLipTracker)lip).deviceStatus == ViveSR.Error.LIP_NOT_SUPPORT)
            {
                _ = lip.Start();
            }
        }

        internal async Task<EyeTracker> ChangeEyeTracker(EyeTrackingType trackingType)
        {
            switch (trackingType)
            {
                case EyeTrackingType.ViveSRanipal:
                    eye = await EyeTracker.Instance<SRanipalEyeTracker>().ConfigureAwait(false);

                    _ = Task.Run(() => {
                        if (SRanipal_Eye.IsViveProEye())
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
                    break;
                case EyeTrackingType.PimaxAsee:
                    eye = await EyeTracker.Instance<DroolonPi1EyeTracker>().ConfigureAwait(false);
                    break;
                case EyeTrackingType.Debug:
                    eye = await EyeTracker.Instance<MockEyeTracker>().ConfigureAwait(false);
                    break;
                default:
                    throw new UnexpectedEnumValueException(trackingType);
            }

            return eye;
        }

        internal async Task<LipTracker> ChangeLipTracker(LipTrackingType trackingType)
        {
            switch (trackingType)
            {
                case LipTrackingType.ViveSRanipal:
                    lip = await LipTracker.Instance<SRanipalLipTracker>().ConfigureAwait(false);
                    break;
                case LipTrackingType.Debug:
                    lip = await LipTracker.Instance<MockLipTracker>().ConfigureAwait(false);
                    break;
                default:
                    throw new UnexpectedEnumValueException(trackingType);
            }

            return lip;
        }
        private void TrackerOnUpdated(Tracker instance, IDictionary<FaceKey, float> rawData, TrackingType type)
        {
            try
            {
                foreach (var o in new TrackingData(rawData, Signals, TrackingStatus.MaxAngle, type).CalcAndGet())
                {
                    osc.Send(o);
                    Signals[o.key].Value = o.value;
                }
            }
            catch (Exception e)
            {
                log.AddLog(e.Source, e.Message);
            }
        }

        bool isModifyingFilter = false;

        internal void ChangeFilter(object sender, SelectionChangedEventArgs args)
        {
            if (isModifyingFilter) return;

            var ui = (ListBox)sender;
            var selected = ui.SelectedItems;

            isModifyingFilter = true;
            if (args.AddedItems.Count == 0 && selected.Count == 0)
            {
                selected.Clear();
                selected.Add(OSCDataFilter.All);
            }
            else if (args.AddedItems.Contains(OSCDataFilter.All))
            {
                selected.Clear();
                selected.Add(OSCDataFilter.All);
            }
            else if (args.AddedItems.Contains(OSCDataFilter.OnlyEnabled))
            {
                selected.Clear();
                selected.Add(OSCDataFilter.OnlyEnabled);
            }
            else
            {
                if (selected.Contains(OSCDataFilter.All))
                {
                    selected.Remove(OSCDataFilter.All);
                }

                if (selected.Contains(OSCDataFilter.OnlyEnabled))
                {
                    selected.Remove(OSCDataFilter.OnlyEnabled);
                }

            }
            isModifyingFilter = false;

            _ = Task.Run(() =>
            {
                var toBeDisplayed = selected
                .Cast<OSCDataFilter>()
                .SelectMany(filter =>
                    Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().Where(k =>
                        filter switch
                        {
                            OSCDataFilter.All => true,
                            OSCDataFilter.OnlyEnabled => Signals[k].IsSending,
                            OSCDataFilter.Essential => FaceKeyUtils.IsEssential(k),
                            OSCDataFilter.EyeRaw => FaceKeyUtils.GetDataType(k) == DataType.Eye,
                            OSCDataFilter.EyeComputed => FaceKeyUtils.GetDataType(k) == DataType.ComputedEye,
                            OSCDataFilter.Gaze => FaceKeyUtils.GetDataType(k) == DataType.Gaze,
                            OSCDataFilter.LipRaw => FaceKeyUtils.GetDataType(k) == DataType.Facial,
                            OSCDataFilter.LipComputed => FaceKeyUtils.GetDataType(k) == DataType.ComputedFacial,
                            _ => throw new UnexpectedEnumValueException(filter),
                        })
                )
                .Distinct()
                .Select(k => Signals[k]);

                TrackingStatus.DisplayingSignalList = new ObservableCollection<SignalProperty>(toBeDisplayed);
            }).ConfigureAwait(false);
        }

        internal void BulkCheck()
        {
            foreach (var signal in TrackingStatus.DisplayingSignalList)
            {
                signal.IsSending = true;
            }
        }
        internal void BulkUnCheck()
        {
            foreach (var signal in TrackingStatus.DisplayingSignalList)
            {
                signal.IsSending = false;
            }
        }

        internal void SetEyeFps(double fps)
        {
            eye?.SetTargetFps(fps);
        }
        internal void SetLipFps(double fps)
        {
            lip?.SetTargetFps(fps);
        }

        internal Task RevertConfigs()
        {
            return InitApp();
        }
        internal Task<(EyeTracker eye, LipTracker lip)> ResetAll()
        {
            return LoadAsync(new PreferencesV2());
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
            }).ConfigureAwait(false);
        }

        internal void MarkDirty()
        {
            Configs.IsDirty = true;
        }
    }
}
