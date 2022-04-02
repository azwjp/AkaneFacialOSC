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
using System.Windows.Threading;
using AZW.FacialOSC.Model;
using AZW.FacialOSC.Service;
using AZW.FacialOSC.Tracking;

namespace AZW.FacialOSC
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
        LipTracker? lip;
        EyeTracker? eye;
        WorkerStatus workerStatus = new WorkerStatus();
        public Rows rows = new Rows();

        OSCService osc = new OSCService();

        internal IDictionary<FaceKey, SignalRow> Signals { get { return rows.originalList; } }

        internal async Task Init(MainWindow mw)
        {
            mainWindow = mw;
            mainWindow.Controller = this;
            mainWindow.DataContext = workerStatus;
            var p = PreferencesV2.Load();
            rows.originalList = p.faceDataPreferences.Select(p => new SignalRow().InitRow(p.FaceKey, p.isSending, p.gain, p.isClipping, p.CenterKey)).ToDictionary(r => r.Key, r => r);
            mainWindow.eyeType.SelectedValue = p.Tracker;

            switch (p.Tracker)
            {
                case EyeTrackingType.ViveSRanipal:
                    eye = await EyeTracker.Instance<SRanipalEyeTracker>();
                    break;
                case EyeTrackingType.PimaxAsee:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            lip = await LipTracker.Instance<SRanipalLipTracker>();
            lip.updatedHandler += () =>
            {
                
            };

            eye.statusChangedHandler += (status) =>
            {
                workerStatus.EyeTrackingStatus = status;
            };
            workerStatus.EyeTrackingStatus = eye.Status;
            lip.statusChangedHandler += (status) =>
            {
                workerStatus.LipTrackingStatus = status;
            };
            workerStatus.LipTrackingStatus = lip.Status;

            var list = new ObservableCollection<SignalRow>(Signals.Select(kv => kv.Value).ToList());
             _= mainWindow.Dispatcher.InvokeAsync(() =>
             {
                 mainWindow.DataContext = workerStatus;
                 mainWindow.SignalList.DataContext = list;
                 mainWindow.FilterList.DataContext = new SwitchableObservableCollection<OSCDataFilter>(Enum.GetValues(typeof(OSCDataFilter)).Cast<OSCDataFilter>().ToList()); ;
                 mainWindow.mainPanel.Visibility = Visibility.Visible;
            });
        }

        internal async void SwitchEyeTracker()
        {
            if (eye == null) eye = await EyeTracker.Instance<SRanipalEyeTracker>().ConfigureAwait(false);

            _ = eye.Switch().ConfigureAwait(false);
        }

        internal async void SwitchFacialTracker()
        {
            if (lip == null) lip = await LipTracker.Instance<SRanipalLipTracker>().ConfigureAwait(false);

            var status = await lip.Switch().ConfigureAwait(false);

            // When it is connected with the wireless kit, it could run in the next attempt even if the device throw an error in the first time
            if (status == DeviceStatus.Unavailable && lip.GetType() == typeof(SRanipalLipTracker) && ((SRanipalLipTracker)lip).deviceStatus == ViveSR.Error.LIP_NOT_SUPPORT)
            {
                _ = lip.Start();
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
                        {
                            switch (filter)
                            {
                                case OSCDataFilter.All:
                                    return true;
                                case OSCDataFilter.OnlyEnabled:
                                    return Signals[k].IsSending;
                                case OSCDataFilter.Essential:
                                    return FaceKeyUtils.IsEssential(k);
                                case OSCDataFilter.EyeRaw:
                                    return FaceKeyUtils.GetDataType(k) == DataType.Eye;
                                case OSCDataFilter.EyeComputed:
                                    return FaceKeyUtils.GetDataType(k) == DataType.ComputedEye;
                                case OSCDataFilter.Gaze:
                                    return FaceKeyUtils.GetDataType(k) == DataType.Gaze;
                                case OSCDataFilter.LipRaw:
                                    return FaceKeyUtils.GetDataType(k) == DataType.Facial;
                                case OSCDataFilter.LipComputed:
                                    return FaceKeyUtils.GetDataType(k) == DataType.ComputedFacial;
                                default:
                                    throw new NotImplementedException();
                            }
                        })
                    )
                    .Distinct()
                    .Select(k => Signals[k]);
                    mainWindow?.Dispatcher.InvokeAsync(() =>
                    {
                        mainWindow.SignalList.DataContext = new ObservableCollection<SignalRow>(toBeDisplayed);
                    });
            }).ConfigureAwait(false);
        }

        internal void Save()
        {
            Task.Run(() =>
            {
                var pref = new PreferencesV2()
                {
                    eyeTrackingType = workerStatus.EyeType.ToString(),
                    faceDataPreferences = rows.originalList.Values.Select(r => new FaceDataPreferencesV2()
                    {
                        key = r.Key.ToString(),
                        range = r.ValueRange.ToString(),
                        gain = r.Gain,
                        isClipping = r.IsClipping,
                        isSending = r.IsSending,
                    }).ToList(),
                    language = workerStatus.Language,
                    //maxAngle,
                }.Save();
                workerStatus.IsDirty = false;
            }).ConfigureAwait(false);
        }

        internal void LanguageChanged(string cultureName)
        {
            Task.Run(() =>
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                ResourceService.Current.ChangeCulture(culture);
                workerStatus.Language = cultureName;
                workerStatus.NotifyPropertyChanged(nameof(workerStatus.EyeTrackingStatus));
                workerStatus.NotifyPropertyChanged(nameof(workerStatus.LipTrackingStatus));
                workerStatus.NotifyPropertyChanged(nameof(workerStatus.IsDirty));
                MarkDirty();
            }).ConfigureAwait(false);
        }

        internal void MarkDirty()
        {
            workerStatus.IsDirty = true;
        }
    }
}
