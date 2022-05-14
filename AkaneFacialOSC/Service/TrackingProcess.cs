using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azw.FacialOsc.Common;
using Azw.FacialOsc.Common.Model;
using Azw.FacialOsc.Tracking;

namespace Azw.FacialOsc.Service
{
    internal abstract class TrackingProcess : IDisposable
    {
        protected const int refreshRateMs = 20;
        public bool IsActive = false;
        protected Controller controller;
        protected Process? tracker;
        protected TrackingType trackingType = TrackingType.Eye;
        protected string deviceName = "";
        CancellationTokenSource? cts;
        SharedData sharedData;
        public delegate void OnStatusChanged(DeviceStatus status, int errorCode);
        public OnStatusChanged? StatusChangedHandler;
        public delegate void CheckingData(TrackingInfo data, IDictionary<FaceKey, float> values);
        public CheckingData? CheckingDataHandler;

        private DeviceStatus lastStatus = DeviceStatus.Disbled;

        public TrackingProcess(Controller c, TrackingType trackingType)
        {
            controller = c;
            this.trackingType = trackingType;
            sharedData = new SharedData(trackingType.ToString(), deviceName, SharedObjectUtils.TrackingDataSuffix, true);
        }

        public void Switch()
        {
            if (tracker == null)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public abstract void Start();

        public void Prestart()
        {
            if (IsActive) return;

            IsActive = true;
            StatusChangedHandler?.Invoke(DeviceStatus.Starting, 0);
            lastStatus = DeviceStatus.Starting;
            cts = new CancellationTokenSource();

            _ = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var (trackingData, values) = sharedData.ReadTrackingData();
                    CheckingDataHandler?.Invoke(trackingData, values);

                    var status = (DeviceStatus)trackingData.Status;

                    if (!IsActive || cts == null || cts.IsCancellationRequested)
                    {
                        return;
                    }
                    else if (lastStatus != status && (status == DeviceStatus.Running || status == DeviceStatus.Unavailable))
                    {
                        StatusChangedHandler?.Invoke((DeviceStatus)trackingData.Status, trackingData.ErrorCode);
                        lastStatus = status;
                    }
                    Thread.Sleep(refreshRateMs);
                }
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);

            PropertyChanged();
        }

        public void Stop()
        {
            IsActive = false;
            PropertyChanged();
            StatusChangedHandler?.Invoke(DeviceStatus.Stopping, 0);
            cts?.Cancel();
            if (tracker != null)
            {
                lock (tracker)
                {
                    tracker.WaitForExit(2000);
                    if (!tracker.HasExited) tracker.Kill();
                    tracker = null;
                }
            }
            StatusChangedHandler?.Invoke(DeviceStatus.Disbled, 0);
        }

        public void PropertyChanged()
        {
            var signals = controller.Signals.OrderBy(kv => (int)kv.Key).Select(kv =>
            {
                var v = kv.Value;
                return new Signal()
                {
                    Key = kv.Key,
                    IsSending = v.IsSending,
                    Gain = v.Gain,
                    Curve = v.Curve,
                    IsClipping = v.IsClipping,
                    Range = v.ValueRange,
                };
            }).ToArray();
            var props = new SharedSignalProps()
            {
                MaxAngleRadian = controller.TrackingStatus.MaxAngleRadian,
                TargetIntervalFps = trackingType switch
                {
                    TrackingType.Eye => controller.TrackingStatus.EyeTrackerTargetFps,
                    _ => controller.TrackingStatus.LipTrackerTargetFps,
                },
                Active = IsActive,
            };

            sharedData.WriteProperties(props, signals);
        }

        public void Dispose()
        {
            Stop();
            sharedData?.Dispose();
            sharedData = null;
        }
    }
}
