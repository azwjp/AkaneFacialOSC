using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Lip;

namespace Azw.FacialOsc.Tracking
{
    internal class MockLipTracker : LipTracker
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

        public override Task<bool> StartProcess()
        {
            return Task.Run(() => running = true);
        }

        public override Task<bool> StopProcess()
        {
            return Task.Run(() => running = false);
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
            var sin = MathF.Sin(radian);
            var sinSq = sin * sin;

            return new Dictionary<FaceKey, float>() {
                { FaceKey.Jaw_Right             , sinSq },
                { FaceKey.Jaw_Left              , sinSq },
                { FaceKey.Jaw_Forward           , sinSq },
                { FaceKey.Jaw_Open              , sinSq },
                { FaceKey.Mouth_Ape_Shape       , sinSq },
                { FaceKey.Mouth_Upper_Right     , sinSq },
                { FaceKey.Mouth_Upper_Left      , sinSq },
                { FaceKey.Mouth_Lower_Right     , sinSq },
                { FaceKey.Mouth_Lower_Left      , sinSq },
                { FaceKey.Mouth_Upper_Overturn  , sinSq },
                { FaceKey.Mouth_Lower_Overturn  , sinSq },
                { FaceKey.Mouth_Pout            , sinSq },
                { FaceKey.Mouth_Smile_Right     , sinSq },
                { FaceKey.Mouth_Smile_Left      , sinSq },
                { FaceKey.Mouth_Sad_Right       , sinSq },
                { FaceKey.Mouth_Sad_Left        , sinSq },
                { FaceKey.Cheek_Puff_Right      , sinSq },
                { FaceKey.Cheek_Puff_Left       , sinSq },
                { FaceKey.Cheek_Suck            , sinSq },
                { FaceKey.Mouth_Upper_UpRight   , sinSq },
                { FaceKey.Mouth_Upper_UpLeft    , sinSq },
                { FaceKey.Mouth_Lower_DownRight , sinSq },
                { FaceKey.Mouth_Lower_DownLeft  , sinSq },
                { FaceKey.Mouth_Upper_Inside    , sinSq },
                { FaceKey.Mouth_Lower_Inside    , sinSq },
                { FaceKey.Mouth_Lower_Overlay   , sinSq },
                { FaceKey.Tongue_LongStep1      , sinSq },
                { FaceKey.Tongue_LongStep2      , sinSq },
                { FaceKey.Tongue_Down           , sinSq },
                { FaceKey.Tongue_Up             , sinSq },
                { FaceKey.Tongue_Right          , sinSq },
                { FaceKey.Tongue_Left           , sinSq },
                { FaceKey.Tongue_Roll           , sinSq },
                { FaceKey.Tongue_UpLeft_Morph   , sinSq },
                { FaceKey.Tongue_UpRight_Morph  , sinSq },
                { FaceKey.Tongue_DownLeft_Morph , sinSq },
                { FaceKey.Tongue_DownRight_Morph, sinSq },
            };
        }
    }
}
