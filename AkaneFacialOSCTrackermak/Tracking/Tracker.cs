using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Azw.FacialOsc.TrackingDataRemoteObject;

namespace Azw.FacialOsc.Tracking
{
    internal abstract class Tracker
    {
        TrackingDataRemoteObject? sharingStatus;

        public DeviceStatus Status
        {
            get { return sharingStatus.Status; }
            set
            {
                sharingStatus.Status = value;
                sharingStatus?.statusChangedHandler(new OnStatusChangedEventArg(value));
            }
        }

        CancellationTokenSource? cts;
        public AverageFps ApplicationFps { get; set; } = new();
        public AverageFps TrackingFps { get; set; } = new();
        public delegate void OnUpdated(IDictionary<FaceKey, float> rawData);
        public OnUpdated? updatedHandler;

        public async Task<DeviceStatus> Start()
        {
            Status = DeviceStatus.Starting;
            await StartProcess().ConfigureAwait(false);
            if (CheckWorking())
            {
                Status = DeviceStatus.Running;

                cts = new CancellationTokenSource();
                _ = Task.Factory.StartNew(Runner, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
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
                sharingStatus?.OnCheckedHandler?.Invoke(new OnDataCheckedEventArg(ApplicationFps.averageFps));
                if (cts == null || cts.IsCancellationRequested) { return; }
                if (UpdateData())
                {
                    updatedHandler?.Invoke(GetData());
                    TrackingFps.Add(updatingTime.Elapsed);
                    updatingTime = Stopwatch.StartNew();
                }
                var sleepTime = sharingStatus.TargetInterval - loopTime.Elapsed;
                if (sleepTime > TimeSpan.Zero) Thread.Sleep(sleepTime);
                ApplicationFps.Add(loopTime.Elapsed);
                TrackingFps.Temporary(updatingTime.Elapsed);
                loopTime = Stopwatch.StartNew();
            }
        }

        public void SetTargetFps(double fps)
        {
            sharingStatus.TargetInterval = fps > 0 ? TimeSpan.FromSeconds(1 / fps) : TimeSpan.Zero;
        }

        protected abstract bool CheckWorking();

        public abstract Task StartProcess();
        public abstract Task StopProcess();

        public abstract bool UpdateData();
        public abstract IDictionary<FaceKey, float> GetData();

    }
}
