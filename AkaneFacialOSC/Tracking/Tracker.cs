using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Tracking
{
    internal abstract class Tracker
    {
        public DeviceStatus _status { get; set; } = DeviceStatus.Disbled;

        public DeviceStatus Status {
            get { return _status; }
            set {
                _status = value;
                statusChangedHandler?.Invoke(this, value);
            }
        }

        CancellationTokenSource? cts;
        public delegate void OnUpdated(Tracker instance, IDictionary<FaceKey, float> rawData);
        public delegate void OnChecked(Tracker instance);
        public delegate void OnStatusChanged(Tracker instance, DeviceStatus status);
        public OnUpdated? updatedHandler;
        public OnChecked? checkedHandler;
        public OnStatusChanged? statusChangedHandler;
        public TimeSpan targetInterval = TimeSpan.FromMilliseconds(1000d / 60d);
        public double ApplicationFps { get; set; } = 0;
        public double TrackingFps { get; set; } = 0;
        public bool IsAutoFpsEnabled = true;

        private TimeSpan longestInterval = TimeSpan.FromSeconds(1);
        private TimeSpan shortestInterval = TimeSpan.FromMilliseconds(1);

        public async Task<DeviceStatus> Start()
        {
            Status = DeviceStatus.Starting;
            await StartProcess().ConfigureAwait(false);
            if (CheckWorking())
            {
                Status = DeviceStatus.Running;

                cts = new CancellationTokenSource();
                _ = Task.Run(Runner, cts.Token).ConfigureAwait(false);
            }
            else
            {
                Status = DeviceStatus.Unavailable;
                await StopProcess();
            }

            return Status;
        }

        public async Task<DeviceStatus> Stop()
        {
            Status = DeviceStatus.Stopping;
            cts?.Cancel();
            await StopProcess();
            Status = DeviceStatus.Disbled;
            return Status;
        }
        void Runner()
        {
            var updatingTime = Stopwatch.StartNew();
            var loopTime = Stopwatch.StartNew();
            while (true)
            {
                var elasped = Stopwatch.StartNew();
                loopTime = Stopwatch.StartNew();
                checkedHandler?.Invoke(this);
                if (cts == null || cts.IsCancellationRequested) { return; }
                if (UpdateData())
                {
                    updatedHandler?.Invoke(this, GetData());
                    TrackingFps = 1 / updatingTime.Elapsed.TotalSeconds;
                    updatingTime = Stopwatch.StartNew();
                    if (IsAutoFpsEnabled)
                    {
                        targetInterval *= 0.99;
                        if (targetInterval < shortestInterval) targetInterval = shortestInterval;
                    }
                }
                else if (IsAutoFpsEnabled) {
                    targetInterval = targetInterval + elasped.Elapsed / 4;
                    if (targetInterval > longestInterval) targetInterval = longestInterval;
                }
                var sleepTime = targetInterval - elasped.Elapsed;
                if (sleepTime > TimeSpan.Zero) Thread.Sleep(sleepTime);
                ApplicationFps = 1 / loopTime.Elapsed.TotalSeconds;
            }
        }

        public void SetTargetFps(double fps)
        {
            targetInterval = fps > 0 ? TimeSpan.FromSeconds(1 / fps) : TimeSpan.Zero;
        }

        protected abstract bool CheckWorking();

        public abstract Task StartProcess();
        public abstract Task StopProcess();

        public abstract bool UpdateData();
        public abstract IDictionary<FaceKey, float> GetData();

        public Task<DeviceStatus> Switch()
        {
            switch (Status)
            {
                case DeviceStatus.Starting:
                case DeviceStatus.Running:
                    return Stop();
                case DeviceStatus.Unavailable:
                case DeviceStatus.Disbled:
                case DeviceStatus.Stopping:
                    return Start();
                default:
                    throw new UnexpectedEnumValueException(Status);
            }
        }
    }
}
