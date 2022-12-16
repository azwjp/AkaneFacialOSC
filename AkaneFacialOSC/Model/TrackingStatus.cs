using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Azw.FacialOsc.Common;

namespace Azw.FacialOsc.Model
{
    internal class TrackingStatus : INotifyPropertyChanged
    {
        public Controller? Controller;
        void SetDirty() { Controller?.MarkDirty(); }

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


        private ObservableCollection<SignalProperty> displayingSignalList = new ObservableCollection<SignalProperty>();
        public ObservableCollection<SignalProperty> DisplayingSignalList
        {
            get { return displayingSignalList; }
            set
            {
                if (displayingSignalList == value) return;
                displayingSignalList = value;
                NotifyPropertyChanged(nameof(DisplayingSignalList));
            }
        }

        public Dictionary<EyeTrackingType, string> EyeTrackerList { get; set; } = Enum.GetValues(typeof(EyeTrackingType)).Cast<EyeTrackingType>().ToDictionary(t => t, t => t.ToString());
        public Dictionary<LipTrackingType, string> LipTrackerList { get; set; } = Enum.GetValues(typeof(LipTrackingType)).Cast<LipTrackingType>().ToDictionary(t => t, t => t.ToString());
        public Dictionary<OSCSignalFilter, string> FilterList { get; set; } = new Dictionary<OSCSignalFilter, string>(Enum.GetValues(typeof(OSCSignalFilter)).Cast<OSCSignalFilter>().ToDictionary(s => s, s => s.ToString()));


        private EyeTrackingType eyeType;
        public EyeTrackingType EyeType
        {
            get { return eyeType; }
            set
            {
                if (eyeType == value) return;
                eyeType = value;
                NotifyPropertyChanged(nameof(EyeType));
                SetDirty();
            }
        }

        private LipTrackingType lipType;
        public LipTrackingType LipType
        {
            get { return lipType; }
            set
            {
                if (lipType == value) return;
                lipType = value;
                NotifyPropertyChanged(nameof(LipType));
                SetDirty();
            }
        }

        private double eyeTrackerTargetFps;
        public double EyeTrackerTargetFps
        {
            get { return eyeTrackerTargetFps; }
            set
            {
                if (eyeTrackerTargetFps == value) return;
                eyeTrackerTargetFps = value;
                Controller?.SetEyeFps(value);
                NotifyPropertyChanged(nameof(EyeTrackerTargetFps));
                SetDirty();
            }
        }

        private double lipTrackerTargetFps;
        public double LipTrackerTargetFps
        {
            get { return lipTrackerTargetFps; }
            set
            {
                if (lipTrackerTargetFps == value) return;
                lipTrackerTargetFps = value;
                Controller?.SetLipFps(value);
                NotifyPropertyChanged(nameof(LipTrackerTargetFps));
                SetDirty();
            }
        }

        private float maxAngle = 45;
        public float MaxAngleRadian { get; private set; } = 45f * MathF.PI / 180f;
        public float MaxAngle
        {
            get { return maxAngle; }
            set
            {
                if (maxAngle == value) return;
                maxAngle = value;
                MaxAngleRadian = value * MathF.PI / 180f;
                NotifyPropertyChanged(nameof(MaxAngle));
                SetDirty();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public PreferencesV2.TrackingPreference ToPreference(ICollection<SignalProperty> Signals)
        {
            return new PreferencesV2.TrackingPreference()
            {
                eyeTrackingType = EyeType.ToString(),
                lipTrackingType = LipType.ToString(),
                eyeFps = EyeTrackerTargetFps,
                lipFps = LipTrackerTargetFps,
                faceDataPreferences = Signals.Select(r => new PreferencesV2.FaceDataPreferences()
                {
                    key = r.Key.ToString(),
                    range = r.ValueRange.ToString(),
                    curve = r.Curve,
                    gain = r.Gain,
                    isClipping = r.IsClipping,
                    isSending = r.IsSending,
                }).ToList(),
            };
        }
    }
}
