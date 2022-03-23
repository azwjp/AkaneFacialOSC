using System;
using System.Threading;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Lip;

namespace AZW.FacialOSC.Tracker
{
    internal class SRanipalLipTracker : LipTracker
    {
        static SRanipalLipTracker? instance = null;

        public Error deviceStatus { get; private set; } = Error.NOT_INITIAL;
        int lastUpdate = -1;
        LipData_v2 lipData = new LipData_v2();



        protected override bool CheckWorking()
        {
            return deviceStatus == Error.WORK;
        }

        public override void StartProcess()
        {
            deviceStatus = SRanipal_API.Initial(SRanipal_Lip.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);


        }

        public override void StopProcess()
        {
            deviceStatus = SRanipal_API.Release(SRanipal_Lip.ANIPAL_TYPE_LIP_V2);
        }
        public override bool UpdateData()
        {
            if (lastUpdate == lipData.frame) return false;
            else lastUpdate = lipData.frame;

            var data = new LipData_v2();
            deviceStatus = SRanipal_Lip.GetLipData_v2(ref data);
            if (deviceStatus != Error.WORK)
            {
                lipData = data;
                return true;
            }
            else
            {
                return false;
            }
        }
        public override LipData_v2 GetData()
        {
            return lipData;
        }
    }
}
