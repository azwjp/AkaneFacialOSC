using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Lip;

namespace Azw.FacialOsc.Tracking
{
    internal class SRanipalLipTracker : Tracker
    {
        public Error deviceStatus { get; private set; } = Error.NOT_INITIAL;
        int lastUpdate = 0;
        LipData_v2 lipData = new LipData_v2();

        public SRanipalLipTracker() { }

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
            var data = new LipData_v2();
            deviceStatus = SRanipal_Lip.GetLipData_v2(ref data);

            if (lastUpdate == data.time) return false;
            
            lastUpdate = data.time;

            if (CheckWorking())
            {
                lipData = data;
                return true;
            }
            else
            {
                return false;
            }
        }
        public override IDictionary<FaceKey, float> GetData()
        {
            return new Dictionary<FaceKey, float>() {
                { FaceKey.Jaw_Right             , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Jaw_Right             ] },
                { FaceKey.Jaw_Left              , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Jaw_Left              ] },
                { FaceKey.Jaw_Forward           , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Jaw_Forward           ] },
                { FaceKey.Jaw_Open              , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Jaw_Open              ] },
                { FaceKey.Mouth_Ape_Shape       , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Ape_Shape       ] },
                { FaceKey.Mouth_Upper_Right     , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Upper_Right     ] },
                { FaceKey.Mouth_Upper_Left      , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Upper_Left      ] },
                { FaceKey.Mouth_Lower_Right     , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_Right     ] },
                { FaceKey.Mouth_Lower_Left      , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_Left      ] },
                { FaceKey.Mouth_Upper_Overturn  , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Upper_Overturn  ] },
                { FaceKey.Mouth_Lower_Overturn  , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_Overturn  ] },
                { FaceKey.Mouth_Pout            , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Pout            ] },
                { FaceKey.Mouth_Smile_Right     , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Smile_Right     ] },
                { FaceKey.Mouth_Smile_Left      , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Smile_Left      ] },
                { FaceKey.Mouth_Sad_Right       , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Sad_Right       ] },
                { FaceKey.Mouth_Sad_Left        , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Sad_Left        ] },
                { FaceKey.Cheek_Puff_Right      , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Cheek_Puff_Right      ] },
                { FaceKey.Cheek_Puff_Left       , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Cheek_Puff_Left       ] },
                { FaceKey.Cheek_Suck            , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Cheek_Suck            ] },
                { FaceKey.Mouth_Upper_UpRight   , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Upper_UpRight   ] },
                { FaceKey.Mouth_Upper_UpLeft    , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Upper_UpLeft    ] },
                { FaceKey.Mouth_Lower_DownRight , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_DownRight ] },
                { FaceKey.Mouth_Lower_DownLeft  , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_DownLeft  ] },
                { FaceKey.Mouth_Upper_Inside    , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Upper_Inside    ] },
                { FaceKey.Mouth_Lower_Inside    , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_Inside    ] },
                { FaceKey.Mouth_Lower_Overlay   , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Mouth_Lower_Overlay   ] },
                { FaceKey.Tongue_LongStep1      , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_LongStep1      ] },
                { FaceKey.Tongue_LongStep2      , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_LongStep2      ] },
                { FaceKey.Tongue_Down           , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_Down           ] },
                { FaceKey.Tongue_Up             , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_Up             ] },
                { FaceKey.Tongue_Right          , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_Right          ] },
                { FaceKey.Tongue_Left           , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_Left           ] },
                { FaceKey.Tongue_Roll           , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_Roll           ] },
                { FaceKey.Tongue_UpLeft_Morph   , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_UpLeft_Morph   ] },
                { FaceKey.Tongue_UpRight_Morph  , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_UpRight_Morph  ] },
                { FaceKey.Tongue_DownLeft_Morph , lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_DownLeft_Morph ] },
                { FaceKey.Tongue_DownRight_Morph, lipData.prediction_data.blend_shape_weight[(int)LipShape_v2.Tongue_DownRight_Morph] },
            };
        }
        public LipData_v2 GetRawData()
        {
            return lipData;
        }
    }
}
