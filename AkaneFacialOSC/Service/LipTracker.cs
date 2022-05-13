using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azw.FacialOsc.Common;
using Azw.FacialOsc.Tracking;

namespace Azw.FacialOsc.Service
{
    internal class LipTracker : TrackingProcess
    {
        public LipTrackingType trackingDevice = LipTrackingType.ViveSRanipal;
        public LipTrackingType TrackingDevice
        {
            get
            {
                return trackingDevice;
            }
            set
            {
                trackingDevice = value;
                deviceName = TrackingDevice.ToString();
            }
        }

        public LipTracker(Controller c) : base(c, TrackingType.Lip)
        {
            deviceName = TrackingDevice.ToString();
        }

        public override void Start()
        {
            Prestart(); 
            tracker = Process.Start(new ProcessStartInfo(SharedObjectUtils.GetTrackingAppPath(), $"{TrackingType.Lip} {TrackingDevice} {Process.GetCurrentProcess().Id}")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
