using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;

namespace AZW.FacialOSC.Tracking
{
    internal class SRanipalEyeTracker : EyeTracker
    {
        static SRanipalEyeTracker? instance = null;

        public Error deviceStatus { get; private set; } = Error.NOT_INITIAL;
        int lastUpdate = -1;
        EyeData_v2 eyeData = new EyeData_v2();
        Thread? worker;

        public SRanipalEyeTracker()
        {

        }
        public static SRanipalEyeTracker Instance
        {
            get
            {
                if (instance == null) return instance = new SRanipalEyeTracker();
                else return instance;
            }
        }

        protected override bool CheckWorking()
        {
            return deviceStatus == Error.WORK;
        }

        public override Task<Error> StartProcess()
        {
            return Task.Run(() => deviceStatus = SRanipal_API.Initial(SRanipal_Eye.ANIPAL_TYPE_EYE_V2, IntPtr.Zero));
        }

        public override Task<Error> StopProcess()
        {
            return Task.Run(() => deviceStatus = SRanipal_API.Release(SRanipal_Eye.ANIPAL_TYPE_EYE_V2));
        }

        public override bool UpdateData()
        {
            if (lastUpdate == eyeData.frame_sequence) return false;
            else lastUpdate = eyeData.frame_sequence;

            var data = new EyeData_v2();
            deviceStatus = SRanipal_Eye.GetEyeData_v2(ref data);
            if (CheckWorking())
            {
                eyeData = data;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override EyeData_v2 GetData()
        {
            return eyeData;
        }

    }
}
