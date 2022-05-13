using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azw.FacialOsc.Common;
using Azw.FacialOsc.Model;
using Azw.FacialOsc.Tracking;

namespace Azw.FacialOsc.Service
{
    internal class EyeTracker : TrackingProcess
    {
        EyeTrackingType trackingDevice = EyeTrackingType.ViveSRanipal;
        public EyeTrackingType TrackingDevice
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
        public EyeTracker(Controller c) : base(c, TrackingType.Eye)
        {
            deviceName = TrackingDevice.ToString();
        }

        public override void Start()
        {
            Prestart();
            tracker = Process.Start(SharedObjectUtils.GetTrackingAppPath(), $"{Tracking.TrackingType.Eye} {TrackingDevice} {Process.GetCurrentProcess().Id}");
        }

    }
}
