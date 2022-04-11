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
        public TimeSpan targetInterval = TimeSpan.FromMilliseconds(1000d / 61d);
        private AverageFps targetFps = new();
        public AverageFps ApplicationFps { get; private set; } = new();
        public AverageFps TrackingFps { get; private set; } = new();

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
            targetFps = new();
            targetFps.Add(targetInterval);
            while (true)
            {
                checkedHandler?.Invoke(this);
                if (cts == null || cts.IsCancellationRequested) { return; }
                if (UpdateData())
                {
                    updatedHandler?.Invoke(this, GetData());
                    TrackingFps.Add(updatingTime.Elapsed);
                    updatingTime = Stopwatch.StartNew();
                }
                var sleepTime = targetInterval - loopTime.Elapsed;
                if (sleepTime > TimeSpan.Zero) Thread.Sleep(sleepTime);
                ApplicationFps.Add(loopTime.Elapsed);
                TrackingFps.Temporary(updatingTime.Elapsed);
                loopTime = Stopwatch.StartNew();
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

        public Task<DeviceStatus> Switch(Action? starting = null, Action? stopping = null)
        {
            switch (Status)
            {
                case DeviceStatus.Starting:
                case DeviceStatus.Running:
                    stopping?.Invoke();
                    return Stop();
                case DeviceStatus.Unavailable:
                case DeviceStatus.Disbled:
                case DeviceStatus.Stopping:
                    starting?.Invoke();
                    return Start();
                default:
                    throw new UnexpectedEnumValueException(Status);
            }
        }
    }
}
