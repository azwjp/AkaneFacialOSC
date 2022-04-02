using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViveSR;

namespace AZW.FacialOSC.Tracking
{
    internal abstract class Tracker<T>
    {
        public DeviceStatus _status { get; set; } = DeviceStatus.Disbled;

        public DeviceStatus Status {
            get { return _status; }
            set {
                _status = value;
                statusChangedHandler?.Invoke(value);
            }
        }

        CancellationTokenSource? cts;
        public delegate void OnUpdated(ICollection<(FaceKey, float)> ps);
        public delegate void OnStatusChanged(DeviceStatus status);
        public OnUpdated? updatedHandler;
        public OnStatusChanged? statusChangedHandler;

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
            while (true)
            {
                if (cts == null || cts.IsCancellationRequested) { return; }
                if (UpdateData()) updatedHandler?.Invoke();
            }
        }

        protected abstract bool CheckWorking();

        public abstract Task StartProcess();
        public abstract Task StopProcess();

        public abstract bool UpdateData();
        public abstract T GetData();

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
                    throw new InvalidOperationException();
            }
        }
    }
}
