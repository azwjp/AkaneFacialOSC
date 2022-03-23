using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AZW.FacialOSC.Tracker
{
    internal abstract class Tracker<T>
    {
        public DeviceStatus Status { get; private set; } = DeviceStatus.Disbled;

        CancellationTokenSource? cts;
        public delegate void OnUpdated();
        public OnUpdated? updatedHandler;
        public void Start()
        {
            Task.Run(() =>
            {
                Status = DeviceStatus.Starting;
                StartProcess();
                if (CheckWorking())
                {
                    Status = DeviceStatus.Enabled;

                    cts = new CancellationTokenSource();
                    Task.Run(Runner, cts.Token);
                }
                else Status = DeviceStatus.Unavailable;
            });
        }

        public void Stop()
        {
            Task.Run(() =>
            {
                Status = DeviceStatus.Stopping;
                cts?.Cancel();
                StopProcess();
                Status = DeviceStatus.Disbled;
            });
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

        public abstract void StartProcess();
        public abstract void StopProcess();

        public abstract bool UpdateData();
        public abstract T GetData();
    }
}
