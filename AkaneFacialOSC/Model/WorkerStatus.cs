using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using AZW.FacialOSC.Tracking;

namespace AZW.FacialOSC.Model
{
    internal class WorkerStatus : INotifyPropertyChanged
    {
        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                NotifyPropertyChanged(nameof(IsDirty));
            }
        }

        private DeviceStatus eyeTrackingStatus;
        public DeviceStatus EyeTrackingStatus
        {
            get { return eyeTrackingStatus; }
            set
            {
                eyeTrackingStatus = value;
                NotifyPropertyChanged(nameof(EyeTrackingStatus));
            }
        }

        private DeviceStatus lipTrackingStatus;
        public DeviceStatus LipTrackingStatus
        {
            get { return lipTrackingStatus; }
            set
            {
                lipTrackingStatus = value;
                NotifyPropertyChanged(nameof(LipTrackingStatus));
            }
        }

        public Dictionary<EyeTrackingType, string> EyeTrackerList { get; set; } = Enum.GetValues(typeof(EyeTrackingType)).Cast<EyeTrackingType>().ToDictionary(t => t, t => t.ToString());

        public string Language = CultureInfo.CurrentCulture.Name;
        public Dictionary<string, string> LanguageList { get; set; } = new Dictionary<string, string>()
        {
            {"en-US",  "English"},
            {"ja-JP",  "日本語"},
            {"ko-KR",  "한국어"},
            {"zh-Hant",  "中文（正體）"},
        };

        private EyeTrackingType eyeType;
        public EyeTrackingType EyeType
        {
            get { return eyeType; }
            set { eyeType = value; NotifyPropertyChanged(nameof(EyeTrackingType)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
