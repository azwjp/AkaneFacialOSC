using Azw.FacialOsc.Common.Model;
using Azw.FacialOsc.Tracking;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Azw.FacialOsc
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TrackingInfo
    {

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 90)]
        //public float[] TrackingData = new float[Enum.GetValues(typeof(FaceKey)).Length]; // index is the int value of FaceKey
        public int Status = (int)DeviceStatus.Disbled;
        public int ErrorCode = -2;
        public double TrackingFps = 0;
        public double ApplicationFps = 0;

        public static int DataLength() {
            return //TrackingData.Length * sizeof(float) +
                sizeof(int) // Status
                + sizeof(int) // ErrorCode
                + sizeof(double) // TrackingFps
                + sizeof(double) // ApplicationFps
            ;
        }

        public const int Offset = 8 * 4;

        public TrackingInfo() { }


        public static ReadOnlySpan<TrackingInfo> Serialize(TrackingInfo obj)
        {
            
            return MemoryMarshal.CreateReadOnlySpan(ref obj, DataLength());
        }

        public static TrackingInfo Deserialize(byte[] data)
        {
            return MemoryMarshal.Read<TrackingInfo>(data);
        }

    }
}
