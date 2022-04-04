using Azw.FacialOsc.Model;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Azw.FacialOsc.Tracking
{
    [TestClass]
    public class UnitTest1
    {

        static readonly IDictionary<FaceKey, float> data = new Dictionary<FaceKey, float>()
        { 
            { FaceKey.Tongue_Left           ,  0.5f}, // Gained
            { FaceKey.Tongue_Right          ,  0.5f}, // Gained and clipped
            { FaceKey.Tongue_Up             ,  1.5f}, // Exceeded
            { FaceKey.Tongue_Down           ,  1.5f}, // Exceeded and clipped
            { FaceKey.Tongue_LongStep1      ,  0.5f}, // Gained and clipped

            { FaceKey.Eye_Left_Blink        ,  0.5f }, // Single regular data
            { FaceKey.Eye_Left_Wide         ,  0.5f },
            { FaceKey.Eye_Left_Right        ,  0.5f },
            { FaceKey.Eye_Left_Left         ,  0.5f },
            { FaceKey.Eye_Left_Up           ,  0.5f },
            { FaceKey.Eye_Left_Down         ,  0.5f },
            { FaceKey.Eye_Right_Blink       ,  0.5f },
            { FaceKey.Eye_Right_Wide        ,  0.5f },
            { FaceKey.Eye_Right_Right       ,  0.5f },
            { FaceKey.Eye_Right_Left        ,  0.5f },
            { FaceKey.Eye_Right_Up          ,  0.5f },
            { FaceKey.Eye_Right_Down        ,  0.5f },
            { FaceKey.Eye_Left_Frown        ,  0.5f },
            { FaceKey.Eye_Right_Frown       ,  0.5f },
            { FaceKey.Eye_Left_Squeeze      ,  0.5f },
            { FaceKey.Eye_Right_Squeeze     ,  0.5f },

            { FaceKey.Gaze_Left_Vertical    ,  0.5f },
            { FaceKey.Gaze_Left_Horizontal  ,  0.5f },
            { FaceKey.Gaze_Right_Vertical   ,  0.5f },
            { FaceKey.Gaze_Right_Horizontal ,  0.5f },
            { FaceKey.Gaze_Vertical         ,  0.5f },
            { FaceKey.Gaze_Horizontal       ,  0.5f },



            { FaceKey.Mouth_Sad_Left        ,  1.0f }, // 
            { FaceKey.Mouth_Sad_Right       , -0.5f }, // Gained
            { FaceKey.Mouth_Smile_Left      ,  1.0f }, // Turned off
            { FaceKey.Mouth_Smile_Right     ,  0.5f }, // Clipped
            { FaceKey.Mouth_Sad_Smile_Left  ,  0.5f },
            { FaceKey.Mouth_Sad_Smile_Right ,  0.5f },
            { FaceKey.Mouth_Sad_Smile       ,  0.5f },
        };
        static readonly IDictionary<FaceKey, OSCData> expected = new Dictionary<FaceKey, OSCData>()
        {
            { FaceKey.Eye_Left_Blink        , new OSCData(FaceKey.Eye_Blink             ,  0.5f) }, // Single regular data
            { FaceKey.Mouth_Sad_Left        , new OSCData(FaceKey.Mouth_Sad_Left        ,  1.0f) },
            { FaceKey.Mouth_Sad_Right       , new OSCData(FaceKey.Mouth_Sad_Right       , -0.5f) },
            { FaceKey.Mouth_Smile_Left      , new OSCData(FaceKey.Mouth_Smile_Left      ,  1.0f) },
            { FaceKey.Mouth_Smile_Right     , new OSCData(FaceKey.Mouth_Smile_Right     ,  0.5f) },
            { FaceKey.Mouth_Sad_Smile_Left  , new OSCData(FaceKey.Mouth_Sad_Smile_Left  ,  0.5f) },
            { FaceKey.Mouth_Sad_Smile_Right , new OSCData(FaceKey.Mouth_Sad_Smile_Right ,  0.5f) },
            { FaceKey.Mouth_Sad_Smile       , new OSCData(FaceKey.Mouth_Sad_Smile       ,  0.5f) },
        };
        static readonly IDictionary<FaceKey, SignalProperty> facePrefs = new Dictionary<FaceKey, SignalProperty>()
        {
            { FaceKey.Eye_Left_Blink             , new SignalProperty().InitRow(FaceKey.Eye_Blink             , true, 1, true, ValueRange.Fixed) },
            { FaceKey.Mouth_Sad_Left        , new SignalProperty().InitRow(FaceKey.Mouth_Sad_Left        , true, 1, true, ValueRange.HalfCentered)},
            { FaceKey.Mouth_Sad_Right       , new SignalProperty().InitRow(FaceKey.Mouth_Sad_Right       , true, 1, true, ValueRange.HalfCentered)},
            { FaceKey.Mouth_Smile_Left      , new SignalProperty().InitRow(FaceKey.Mouth_Smile_Left      , true, 1, true, ValueRange.HalfCentered)},
            { FaceKey.Mouth_Smile_Right     , new SignalProperty().InitRow(FaceKey.Mouth_Smile_Right     , true, 1, true, ValueRange.HalfCentered)},
            { FaceKey.Mouth_Sad_Smile_Left  , new SignalProperty().InitRow(FaceKey.Mouth_Sad_Smile_Left  , true, 1, true, ValueRange.HalfCentered)},
            { FaceKey.Mouth_Sad_Smile_Right , new SignalProperty().InitRow(FaceKey.Mouth_Sad_Smile_Right , true, 1, true, ValueRange.HalfCentered)},
            { FaceKey.Mouth_Sad_Smile       , new SignalProperty().InitRow(FaceKey.Mouth_Sad_Smile       , true, 1, true, ValueRange.HalfCentered)},
        };
        const float maxAngleRadian = 45;

        TrackingDataTestTarget target = new TrackingDataTestTarget(data, facePrefs, maxAngleRadian, TrackingType.Lip);

        [TestMethod]
        public void TestCalc()
        {
            target.Calc(FaceKey.Eye_Left_Blink)?.Should().Be(new OSCData(FaceKey.Eye_Left_Blink, 0.5f));
        }


        internal class TrackingDataTestTarget : TrackingData
        {
            public TrackingDataTestTarget(
                IDictionary<FaceKey, float> data,
                IDictionary<FaceKey, SignalProperty> facePrefs,
                float maxAngleRadian,
                TrackingType calcType
            ) : base(data, facePrefs, maxAngleRadian, calcType) { }

            public new OSCData? Calc(FaceKey key)
            {
                return base.Calc(key);
            }
            public new OSCData? CalcBipolar(FaceKey key, FaceKey lowerKey, FaceKey higherKey)
            {
                return base.CalcBipolar(key, lowerKey, higherKey);
            }

            public new OSCData? CalcAverage(FaceKey key, FaceKey key1, FaceKey key2)
            {
                return base.CalcAverage(key, key1, key2);
            }

            public new OSCData? CalcAveragedBipolar(FaceKey unitedKey, FaceKey[] lowerKeys, FaceKey[] higherKeys)
            {
                return base.CalcAveragedBipolar(unitedKey, lowerKeys, higherKeys);
            }

            public new OSCData? CalcRotation(FaceKey key)
            {
                return base.CalcRotation(key);
            }

            public new IEnumerable<OSCData?> CalcRotationWithAverage(FaceKey key1, FaceKey key2, FaceKey keyAvarage)
            {
                return base.CalcRotationWithAverage(key1, key2, keyAvarage);
            }
            public new IEnumerable<OSCData?> CalcWithAverageAndBipolar(
               FaceKey unitedKey,
               FaceKey lowerKey, FaceKey higherKey,
               FaceKey key1, FaceKey key2,
               FaceKey lowerKey1, FaceKey lowerKey2, FaceKey higherKey1, FaceKey higherKey2
            )
            {

                return base.CalcWithAverageAndBipolar(unitedKey, lowerKey, higherKey, key1, key2,lowerKey1, lowerKey2, higherKey1, higherKey2);
            }
        }
    }
}