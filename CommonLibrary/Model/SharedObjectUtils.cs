using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Common
{
    public class SharedObjectUtils
    {
        public const string AppName = "AZW_AkaneFacialOSC";
        public const string SignalPropsSuffix = "Signals";
        public const string TrackingInfoSuffix = "TrackingInfo";
        public const string TrackingDataSuffix = "TrackingData";
        const string TrackingAppName = "AkaneFacialOSCTracker.exe";
        public static string GetTrackingAppPath()
        {
            var pwd = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            return $"{pwd}\\{TrackingAppName}";
    }
    }
}
