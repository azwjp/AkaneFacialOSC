using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Common.Model
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct SharedSignalProps
    {
        //public Signal[] Props = new Signal[Enum.GetValues(typeof(FaceKey)).Length]; // index is the int value of FaceKey
        public float MaxAngleRadian = 0;
        public double TargetIntervalFps = 61;
        public bool Active = true;
        public SharedSignalProps() { }
    }
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Signal
    {
        public FaceKey Key;
        public bool IsSending;
        public float Gain;
        public float Curve;
        public bool IsClipping;
        public ValueRange Range;

        public float CenterValue()
        {
            return Range switch
            {
                ValueRange.ZeroToOne => 0.5f,
                ValueRange.MinusOneToOne => 0,
                _ => throw new UnexpectedEnumValueException(Range),
            };
        }
    }
}
