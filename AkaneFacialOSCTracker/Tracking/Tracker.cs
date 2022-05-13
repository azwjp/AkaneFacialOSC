using Azw.FacialOsc.Common.Model;
using System.Diagnostics;

namespace Azw.FacialOsc.Tracking
{
    internal abstract class Tracker
    {
        //public TrackingDataRemoteObject SharingStatus = new TrackingDataRemoteObject();
        public SharedSignalProps Properties = new SharedSignalProps();

        private DeviceStatus status;

        public DeviceStatus Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    StatusChangedHandler?.Invoke(this);
                }
            }
        }

        CancellationTokenSource? cts;
        public TimeSpan TargetInterval { get; set; } = TimeSpan.FromSeconds(1000d / 61d);
        public AverageFps ApplicationFps { get; set; } = new();
        public AverageFps TrackingFps { get; set; } = new();
        public delegate void OnUpdatedValue(Tracker instance, IDictionary<FaceKey, float> rawData);
        public OnUpdatedValue? UpdatedValueHandler;
        public delegate void OnStatusChanged(Tracker instance);
        public OnStatusChanged? StatusChangedHandler;

        public async Task StartAsync()
        {
            Status = DeviceStatus.Starting;
            await Task.Run(() => StartProcess()).ConfigureAwait(false);
            if (CheckWorking())
            {
                Status = DeviceStatus.Running;

                cts = new CancellationTokenSource();
                _ = Task.Factory.StartNew(Runner, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
            }
            else
            {
                Status = DeviceStatus.Unavailable;
                _ = Task.Run(() => StopProcess()).ConfigureAwait(false);
            }
        }
        public void Start()
        {
            Status = DeviceStatus.Starting;
            StartProcess();
            if (CheckWorking())
            {
                cts = new CancellationTokenSource();
                Status = DeviceStatus.Running;
                Runner();
            }
            else
            {
                Status = DeviceStatus.Unavailable;
                StopProcess();
            }
        }

        public async Task<DeviceStatus> StopAsync()
        {
            Status = DeviceStatus.Stopping;
            cts?.Cancel();
            await Task.Run(() => StopProcess()).ConfigureAwait(false);
            Status = DeviceStatus.Disbled;
            return Status;
        }
        public DeviceStatus Stop()
        {
            Status = DeviceStatus.Stopping;
            cts?.Cancel();
            StopProcess();
            Status = DeviceStatus.Disbled;
            return Status;
        }
        void Runner()
        {
            var updatingTime = Stopwatch.StartNew();
            var loopTime = Stopwatch.StartNew();
            while (true)
            {
                StatusChangedHandler?.Invoke(this);
                if (cts != null && cts.IsCancellationRequested) { return; }
                if (UpdateData())
                {
                    UpdatedValueHandler?.Invoke(this, GetData());
                    TrackingFps.Add(updatingTime.Elapsed);
                    updatingTime = Stopwatch.StartNew();
                }
                else
                {
                    TrackingFps.Temporary(updatingTime.Elapsed);
                }
                var sleepTime = TargetInterval - loopTime.Elapsed;
                if (sleepTime > TimeSpan.Zero) Thread.Sleep(sleepTime);
                ApplicationFps.Add(loopTime.Elapsed);
                loopTime = Stopwatch.StartNew();
            }
        }

        public void SetTargetFps(double fps)
        {
            TargetInterval = fps > 0 ? TimeSpan.FromSeconds(1 / fps) : TimeSpan.Zero;
        }

        protected abstract bool CheckWorking();

        public abstract void StartProcess();
        public abstract void StopProcess();

        public abstract bool UpdateData();
        public abstract IDictionary<FaceKey, float> GetData();

    }
}
