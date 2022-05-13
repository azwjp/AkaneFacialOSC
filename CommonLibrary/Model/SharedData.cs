using Azw.FacialOsc.Common;
using Azw.FacialOsc.Common.Model;
using Azw.FacialOsc.Tracking;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Azw.FacialOsc
{
    public class SharedData : IDisposable
    {
        bool isUsingMmf = false;
        MemoryMappedFile propsMmf;
        MemoryMappedFile trackingDataMmf;

        const int Size = 1024 * 8;

        public SharedData(string trackingType, string deviceName, string parentProcess, bool isServer)
        {
            if (isServer)
            {
                try
                {
                    propsMmf = MemoryMappedFile.CreateOrOpen(trackingType + SharedObjectUtils.SignalPropsSuffix, Size);
                    trackingDataMmf = MemoryMappedFile.CreateOrOpen(trackingType + SharedObjectUtils.TrackingDataSuffix, Size);
                    isUsingMmf = true;
                } catch { }
            }
            else
            {
                try
                {
                    propsMmf = MemoryMappedFile.OpenExisting(trackingType + SharedObjectUtils.SignalPropsSuffix);
                    trackingDataMmf = MemoryMappedFile.OpenExisting(trackingType + SharedObjectUtils.TrackingDataSuffix);
                    isUsingMmf = true;
                }
                catch { }
            }
        }

        #region FOR_CLIENT
        public (SharedSignalProps props, Signal[] signals) GetProps()
        {
            if (isUsingMmf)
            {
                using (var propsAccessor = propsMmf.CreateViewAccessor())
                {
                    SharedSignalProps props = new SharedSignalProps();
                    propsAccessor.Read(0, out props);
                    var signals = new Signal[Enum.GetValues(typeof(FaceKey)).Length];
                    propsAccessor.ReadArray(32, signals, 0, signals.Length);

                    return (props, signals);
                }
            }
            else
            {
                var props = new SharedSignalProps()
                {
                    MaxAngleRadian = 45 / MathF.PI,
                    TargetIntervalFps = 61,
                };
                var signals = Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().Select(k => new Signal()
                {
                    Key = k,
                    Curve = 1,
                    Gain = 1,
                    IsClipping = true,
                    IsSending = true,
                    Range = FaceKeyUtils.GetDataType(k) == DataType.Gaze ? ValueRange.MinusOneToOne : ValueRange.ZeroToOne
                }).ToArray();
                return (props, signals);
            }
        }

        public void WriteTrackingValues(float[] data)
        {
            if (!isUsingMmf) return;

            using (var trackingDataAccessor = trackingDataMmf.CreateViewAccessor())
            {
                trackingDataAccessor.WriteArray(TrackingInfo.Offset, data, 0, data.Length);
            }
        }
        public void WriteTrackingInfo(TrackingInfo trackingInfo)
        {
            if (!isUsingMmf) return;

            using (var trackingDataAccessor = trackingDataMmf.CreateViewAccessor())
            {
                trackingDataAccessor.Write(0, ref trackingInfo);
            }
        }

        #endregion

        #region FOR_SERVER
        public (TrackingInfo, Dictionary<FaceKey, float>) ReadTrackingData()
        {
            var trackingData = new TrackingInfo();
            var values = new float[Enum.GetValues(typeof(FaceKey)).Length];

            if (isUsingMmf)
            {
                using (var trackingDataAccessor = trackingDataMmf.CreateViewAccessor())
                {
                    trackingDataAccessor.Read(0, out trackingData);
                    trackingDataAccessor.ReadArray(TrackingInfo.Offset, values, 0, values.Length);
                }
            }

            return (trackingData, values.Select((v, index) => ((FaceKey)index, v)).ToDictionary(e => e.Item1, e => e.v));
        }
        public void WriteProperties(SharedSignalProps props, Signal[] signals)
        {
            if (!isUsingMmf) return;

            using (var propsAccessor = propsMmf?.CreateViewAccessor())
            {
                propsAccessor?.Write(0, ref props);
                propsAccessor?.WriteArray(32, signals, 0, signals.Length);
            }
        }

        #endregion
        public void Dispose()
        {
            if (!isUsingMmf) return;
            propsMmf.Dispose();
            trackingDataMmf.Dispose();
        }
    }
}
