using Azw.FacialOsc.Model;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Azw.FacialOsc.Tracking
{
    internal class TestTrackingSignal
    {
        [TestMethod]
        public void TestTracking()
        {
            var ts = new OSCData(FaceKey.Eye_Left_Blink, 1);
            ts.key.Should().Be(FaceKey.Eye_Left_Blink);
            ts.value.Should().Be(1);
        }
        [TestMethod]
        public void TestTrackingSignalAdd()
        {
            var x = new OSCData(FaceKey.Eye_Left_Blink, 1);
            var y = new OSCData(FaceKey.Eye_Left_Blink, 2);
            (x + y).Should().Be(3);
        }
        [TestMethod]
        public void TestFps()
        {
            AverageFps.CulcNewCapacity(60).Should().Be(60);
            AverageFps.CulcNewCapacity(61).Should().Be(60);
            AverageFps.CulcNewCapacity(59).Should().Be(60);
        }
    }
}
