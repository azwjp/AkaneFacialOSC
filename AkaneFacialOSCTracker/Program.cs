// See https://aka.ms/new-console-template for more information
using Azw.FacialOsc;
using Azw.FacialOsc.Service;
using Azw.FacialOsc.Tracking;
using System.Diagnostics;

var trackingType = (TrackingType)Enum.Parse(typeof(TrackingType), args[0]);
Tracker tracker = trackingType switch
{
    TrackingType.Eye => Enum.Parse(typeof(EyeTrackingType), args[1]) switch
    {
        EyeTrackingType.ViveSRanipal => new SRanipalEyeTracker(),
        EyeTrackingType.PimaxAsee => new DroolonPi1EyeTracker(),
        _ => new MockEyeTracker(),
    },
    _ => Enum.Parse(typeof(LipTrackingType), args[1]) switch
    {
        LipTrackingType.ViveSRanipal => new SRanipalLipTracker(),
        _ => new MockLipTracker(),
    },
};
var parent = args.Length > 2 ? args[2] : "";

OSCService osc = new();
var conf = new SharedData(args[0], args[1], parent, false);

tracker.StatusChangedHandler += instance =>
{
    try
    {
        var (props, signals) = conf.GetProps();
        if (instance.Status == DeviceStatus.Running
            && (
                !props.Active
                || (!string.IsNullOrWhiteSpace(parent) && Process.GetProcessById(int.Parse(parent)).HasExited)
            )
        ) instance.Stop();

        instance.Properties = props;
        instance.SetTargetFps(props.TargetIntervalFps);

        TrackingInfo trackingInfo = new TrackingInfo()
        {
            ApplicationFps = instance.ApplicationFps.averageFps,
            TrackingFps = instance.TrackingFps.averageFps,
            Status = (int)instance.Status
        };
        switch (instance)
        {
            case SRanipalEyeTracker s:
                trackingInfo.ErrorCode = (int)s.deviceStatus;
                break;
        }

        conf.WriteTrackingInfo(trackingInfo);
    }
    catch
    {
        instance.Stop();
    }
};

tracker.UpdatedValueHandler += (instance, rawData) =>
{
    try
    {
        var (props, signals) = conf.GetProps();

        var facePreps = signals.Select((sig, index) => (sig, index)).ToDictionary(e => (FaceKey)e.index, e => e.sig);
        var oscData = new TrackingData(rawData, facePreps, props.MaxAngleRadian, trackingType)
                .CalcAndGet()
                .ToDictionary(data => data.key, data => data);
        osc.Send(oscData.Values);
        var sharingData = Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().Select(key => oscData.ContainsKey(key) ? oscData[key].value : float.MinValue).ToArray();
        conf.WriteTrackingValues(sharingData);
    }
    catch (Exception e)
    {
    }
};
tracker.Start();

if (tracker.Status == DeviceStatus.Unavailable && tracker is SRanipalLipTracker srLip && srLip.deviceStatus == ViveSR.Error.LIP_NOT_SUPPORT)
{
    tracker.Start();
}

conf.Dispose();