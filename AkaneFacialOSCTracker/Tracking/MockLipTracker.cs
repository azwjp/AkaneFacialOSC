using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azw.FacialOsc.Tracking
{
    internal class MockLipTracker : Tracker
    {
        static MockLipTracker? instance = null;

        bool running = false;
        nuint step = 0;

        public MockLipTracker() { }

        public static MockLipTracker Instance
        {
            get
            {
                if (instance == null) return instance = new MockLipTracker();
                else return instance;
            }
        }

        protected override bool CheckWorking()
        {
            return running;
        }

        public override void StartProcess()
        {
            running = true;
        }

        public override void StopProcess()
        {
            running = false;
        }

        public override bool UpdateData()
        {
            if (step == nuint.MaxValue) step = 0;
            else step++;

            return true;
        }
        public override IDictionary<FaceKey, float> GetData()
        {
            var radian = step / 128f;
            var (sin, cos) = MathF.SinCos(radian);
            var sinSq = sin * sin;
            var positiveSin = MathF.Max(0, sin);
            var positiveCos = MathF.Max(0, cos);

            return new Dictionary<FaceKey, float>() {
                { FaceKey.Jaw_Right             , positiveSin },
                { FaceKey.Jaw_Left              , positiveCos },
                { FaceKey.Jaw_Forward           , sinSq },
                { FaceKey.Jaw_Open              , sinSq },
                { FaceKey.Mouth_Ape_Shape       , sinSq },
                { FaceKey.Mouth_Upper_Right     , positiveSin },
                { FaceKey.Mouth_Upper_Left      , positiveCos },
                { FaceKey.Mouth_Lower_Right     , positiveSin },
                { FaceKey.Mouth_Lower_Left      , positiveCos },
                { FaceKey.Mouth_Upper_Overturn  , positiveSin },
                { FaceKey.Mouth_Lower_Overturn  , positiveSin },
                { FaceKey.Mouth_Pout            , sinSq },
                { FaceKey.Mouth_Smile_Right     , positiveSin },
                { FaceKey.Mouth_Smile_Left      , positiveSin },
                { FaceKey.Mouth_Sad_Right       , positiveCos },
                { FaceKey.Mouth_Sad_Left        , positiveCos },
                { FaceKey.Cheek_Puff_Right      , sinSq },
                { FaceKey.Cheek_Puff_Left       , sinSq },
                { FaceKey.Cheek_Suck            , sinSq },
                { FaceKey.Mouth_Upper_UpRight   , sinSq },
                { FaceKey.Mouth_Upper_UpLeft    , sinSq },
                { FaceKey.Mouth_Lower_DownRight , sinSq },
                { FaceKey.Mouth_Lower_DownLeft  , sinSq },
                { FaceKey.Mouth_Upper_Inside    , positiveCos },
                { FaceKey.Mouth_Lower_Inside    , positiveCos },
                { FaceKey.Mouth_Lower_Overlay   , sinSq },
                { FaceKey.Tongue_LongStep1      , sinSq },
                { FaceKey.Tongue_LongStep2      , sinSq },
                { FaceKey.Tongue_Down           , positiveSin },
                { FaceKey.Tongue_Up             , positiveCos },
                { FaceKey.Tongue_Right          , positiveSin },
                { FaceKey.Tongue_Left           , positiveCos },
                { FaceKey.Tongue_Roll           , sinSq },
                { FaceKey.Tongue_UpLeft_Morph   , positiveSin },
                { FaceKey.Tongue_UpRight_Morph  , positiveCos },
                { FaceKey.Tongue_DownLeft_Morph , positiveSin },
                { FaceKey.Tongue_DownRight_Morph, positiveCos },
            };
        }
    }
}
