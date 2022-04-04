using System;
using System.Collections.Generic;
using System.Linq;
using Azw.FacialOsc.Model;

namespace Azw.FacialOsc.Tracking
{
    public partial class TrackingData : TrackingDataInternal
    {

        private readonly TrackingType calcType;

        public TrackingData(
            IDictionary<FaceKey, float> data,
            IDictionary<FaceKey, SignalProperty> facePrefs,
            float maxAngleRadian,
            TrackingType calcType
        ) : base(data, facePrefs, maxAngleRadian) {
            this.calcType = calcType;
        }

        public IEnumerable<OSCData> CalcAndGet(){
            return calcType switch
            {
                TrackingType.Eye => CalcEyeData(),
                TrackingType.Lip => CalcLipData(),
                _ => throw new UnexpectedEnumValueException(calcType),
            };
        }

        IEnumerable<OSCData> CalcEyeData()
        {
            // Gaze
            CalcRotationWithAverage(FaceKey.Gaze_Left_Horizontal, FaceKey.Gaze_Right_Horizontal, FaceKey.Gaze_Horizontal);
            CalcRotationWithAverage(FaceKey.Gaze_Left_Vertical, FaceKey.Gaze_Right_Vertical, FaceKey.Gaze_Vertical);

            // Skin
            CalcWithAverage(FaceKey.Eye_Up, FaceKey.Eye_Left_Up, FaceKey.Eye_Right_Up);
            CalcWithAverage(FaceKey.Eye_Down, FaceKey.Eye_Left_Down, FaceKey.Eye_Right_Down);
            CalcWithAverage(FaceKey.Eye_Left, FaceKey.Eye_Left_Left, FaceKey.Eye_Right_Left);
            CalcWithAverage(FaceKey.Eye_Right, FaceKey.Eye_Left_Right, FaceKey.Eye_Right_Right);
            CalcWithAverage(FaceKey.Eye_Blink, FaceKey.Eye_Left_Blink, FaceKey.Eye_Right_Blink);
            CalcWithAverage(FaceKey.Eye_Wide, FaceKey.Eye_Left_Wide, FaceKey.Eye_Right_Wide);
            CalcWithAverage(FaceKey.Eye_Frown, FaceKey.Eye_Left_Frown, FaceKey.Eye_Right_Frown);
            CalcWithAverage(FaceKey.Eye_Squeeze, FaceKey.Eye_Left_Squeeze, FaceKey.Eye_Right_Squeeze);

            return calculated;
        }



        IEnumerable<OSCData> CalcLipData()
        {
            Calc(FaceKey.Jaw_Right);
            Calc(FaceKey.Jaw_Left);
            Calc(FaceKey.Jaw_Forward);
            Calc(FaceKey.Jaw_Open);

            Calc(FaceKey.Mouth_Upper_Right);
            Calc(FaceKey.Mouth_Upper_Left);
            Calc(FaceKey.Mouth_Lower_Right);
            Calc(FaceKey.Mouth_Lower_Left);

            Calc(FaceKey.Mouth_Ape_Shape);

            Calc(FaceKey.Mouth_Upper_Overturn);
            Calc(FaceKey.Mouth_Lower_Overturn);

            Calc(FaceKey.Mouth_Pout);

            Calc(FaceKey.Mouth_Upper_UpRight);
            Calc(FaceKey.Mouth_Upper_UpLeft);
            Calc(FaceKey.Mouth_Lower_DownRight);
            Calc(FaceKey.Mouth_Lower_DownLeft);
            Calc(FaceKey.Mouth_Upper_Inside);
            Calc(FaceKey.Mouth_Lower_Inside);
            Calc(FaceKey.Mouth_Lower_Overlay);

            Calc(FaceKey.Tongue_LongStep1);
            Calc(FaceKey.Tongue_LongStep2);
            Calc(FaceKey.Tongue_Down);
            Calc(FaceKey.Tongue_Up);
            Calc(FaceKey.Tongue_Right);
            Calc(FaceKey.Tongue_Left);
            Calc(FaceKey.Tongue_Roll);
            Calc(FaceKey.Tongue_UpLeft_Morph);
            Calc(FaceKey.Tongue_UpRight_Morph);
            Calc(FaceKey.Tongue_DownLeft_Morph);
            Calc(FaceKey.Tongue_DownRight_Morph);

            // Calculated
            CalcBipolar(FaceKey.Jaw_Left_Right, FaceKey.Jaw_Left, FaceKey.Jaw_Right);
            CalcWithAverageAndBipolar(
                FaceKey.Mouth_Sad_Smile,
                FaceKey.Mouth_Sad, FaceKey.Mouth_Smile,
                FaceKey.Mouth_Sad_Smile_Left, FaceKey.Mouth_Sad_Smile_Right,
                FaceKey.Mouth_Sad_Left, FaceKey.Mouth_Sad_Right, FaceKey.Mouth_Smile_Left, FaceKey.Mouth_Smile_Right
            );
            CalcWithAverageAndBipolar(
                FaceKey.Mouth_Left_Right,
                FaceKey.Mouth_Left, FaceKey.Mouth_Right,
                FaceKey.Mouth_Lower_Left_Right, FaceKey.Mouth_Upper_Left_Right,
                FaceKey.Mouth_Lower_Left, FaceKey.Mouth_Upper_Left, FaceKey.Mouth_Lower_Right, FaceKey.Mouth_Upper_Right
            );
            
            CalcBipolar(FaceKey.Mouth_Upper_Inside_Overturn, FaceKey.Mouth_Upper_Inside, FaceKey.Mouth_Upper_Overturn);
            CalcBipolar(FaceKey.Mouth_Lower_Inside_Overturn, FaceKey.Mouth_Lower_Inside, FaceKey.Mouth_Lower_Overturn);

            Calc(FaceKey.Cheek_Suck);
            CalcWithAverage(FaceKey.Cheek_Puff, FaceKey.Cheek_Puff_Left, FaceKey.Cheek_Puff_Right);
            CalcAveragedBipolar(FaceKey.Cheek_Suck_Puff, new FaceKey[] { FaceKey.Cheek_Suck }, new FaceKey[] { FaceKey.Cheek_Puff_Left, FaceKey.Cheek_Puff_Right });

            CalcAverage(FaceKey.Mouth_Upper_Up, FaceKey.Mouth_Upper_UpLeft, FaceKey.Mouth_Upper_UpRight);

            CalcAverage(FaceKey.Mouth_Lower_Down, FaceKey.Mouth_Lower_DownLeft, FaceKey.Mouth_Lower_DownRight);

            CalcBipolar(FaceKey.Tongue_Left_Right, FaceKey.Tongue_Left, FaceKey.Tongue_Right);
            CalcBipolar(FaceKey.Tongue_Down_Up, FaceKey.Tongue_Down, FaceKey.Tongue_Up);

            return calculated;
        }

        IEnumerable<OSCData?> CalcWithAverage(FaceKey keyAverage, FaceKey key1, FaceKey key2)
        {
            return new OSCData?[]{
                Calc(key1),
                Calc(key2),
                CalcAverage(keyAverage, key1, key2),
            };
        }


    }
    public class TrackingDataInternal
    {
        private readonly IDictionary<FaceKey, float> data;
        private readonly IDictionary<FaceKey, SignalProperty> facePrefs;
        private readonly float maxAngleRadian;
        protected ICollection<OSCData> calculated = new List<OSCData>();

        public TrackingDataInternal(
            IDictionary<FaceKey, float> data,
            IDictionary<FaceKey, SignalProperty> facePrefs,
            float maxAngleRadian
        )
        {
            this.data = data;
            this.facePrefs = facePrefs;
            this.maxAngleRadian = maxAngleRadian;
        }
        OSCData? Calc(FaceKey key, float value)
        {
            if (facePrefs.ContainsKey(key))
            {
                var pref = facePrefs[key];
                var center = pref.CenterValue;
                value = (value - center) * pref.Gain + center;
                if (pref.IsClipping)
                {
                    switch (pref.ValueRange)
                    {
                        case ValueRange.ZeroCentered:
                            value = Math.Clamp(value, -1, 1);
                            break;
                        case ValueRange.HalfCentered:
                        case ValueRange.Fixed:
                            value = Math.Clamp(value, 0, 1);
                            break;
                    }
                }
                //            uiRows.SetValue(key, value);
                if (!pref.IsSending) return null;
            }
            var ts = new OSCData(key, value);
            calculated.Add(ts);
            return ts;
        }

        protected OSCData? Calc(FaceKey key)
        {
            return Calc(key, data[key]);
        }
        protected OSCData? CalcBipolar(FaceKey key, FaceKey lowerKey, FaceKey higherKey)
        {
            if (!data.ContainsKey(higherKey) || data.ContainsKey(lowerKey)) return null;

            var higherValue = data[higherKey];
            var lowerValue = data[lowerKey];

            return Calc(
                key,
                higherValue > lowerValue ? higherValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
            );
        }

        protected OSCData? CalcAverage(FaceKey key, FaceKey key1, FaceKey key2)
        {
            if (!data.ContainsKey(key1) || !data.ContainsKey(key2)) return null;

            var average = (data[key1] + data[key2]) / 2.0f;
            return Calc(key, average);
        }
        protected OSCData? CalcAveragedBipolar(FaceKey unitedKey, FaceKey[] lowerKeys, FaceKey[] higherKeys)
        {
            higherKeys = higherKeys.Where(k => data.ContainsKey(k)).ToArray();
            lowerKeys = lowerKeys.Where(k => data.ContainsKey(k)).ToArray();
            if (lowerKeys.Length == 0 || higherKeys.Length == 0) return null;

            var higherValue = higherKeys.Sum(k => data[k]) / higherKeys.Length;
            var lowerValue = lowerKeys.Sum(k => data[k]) / lowerKeys.Length;
            var center = facePrefs.ContainsKey(unitedKey) ? facePrefs[unitedKey].CenterValue : 0.5f; // to make the centre value is 0.5 or the configured value

            return Calc(
                unitedKey,
                higherValue > lowerValue ? higherValue / 2.0f + center : -lowerValue / 2.0f + center
            );
        }

        protected OSCData? CalcRotation(FaceKey key)
        {
            if (!data.ContainsKey(key)) return null;

            var rotation = data[key];
            if (rotation > maxAngleRadian) rotation = maxAngleRadian;
            else if (rotation < -maxAngleRadian) rotation = -maxAngleRadian;
            rotation += facePrefs.ContainsKey(key) ? facePrefs[key].CenterValue : 0.5f; // to make the centre value is 0.5 or the configured value

            return Calc(key, rotation / maxAngleRadian);
        }

        protected IEnumerable<OSCData?> CalcRotationWithAverage(FaceKey key1, FaceKey key2, FaceKey keyAvarage)
        {
            var rotation1 = CalcRotation(key1);
            var rotation2 = CalcRotation(key2);

            if (rotation1 != null && rotation2 != null)
            {
                return new OSCData?[]{
                    rotation1,
                    rotation2,
                    Calc(keyAvarage, (rotation1 + rotation2) / 2),
                };
            }
            else
            {
                return new OSCData?[]{
                    rotation1,
                    rotation2,
                };
            }
        }
        protected IEnumerable<OSCData?> CalcWithAverageAndBipolar(
           FaceKey unitedKey,
           FaceKey lowerKey, FaceKey higherKey,
           FaceKey key1, FaceKey key2,
           FaceKey lowerKey1, FaceKey lowerKey2, FaceKey higherKey1, FaceKey higherKey2
        )
        {

            return new OSCData?[]
            {
                // Single values
                Calc(higherKey1),
                Calc(higherKey2),
                Calc(lowerKey1),
                Calc(lowerKey2),

                // Bipolar
                CalcBipolar(key1, lowerKey1, higherKey1),
                CalcBipolar(key2, lowerKey2, higherKey2),

                // Average
                CalcAverage(higherKey, higherKey1, higherKey2),
                CalcAverage(lowerKey, lowerKey1, lowerKey2),

                // Bipolar with average
                CalcAveragedBipolar(unitedKey, new FaceKey[]{lowerKey1, lowerKey2 }, new FaceKey[]{ higherKey1, higherKey2 }),
            };
        }
    }

    public record class OSCData
    {
        public FaceKey key;
        public float value;
        public OSCData(FaceKey key, float value)
        {
            this.key = key;
            this.value = value;
        }
        public static float operator +(OSCData x, OSCData y)
        {
            return x.value + y.value;
        }
    }
}
