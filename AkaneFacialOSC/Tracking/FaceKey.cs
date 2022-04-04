using System;

namespace Azw.FacialOsc
{
    public enum FaceKey
    {
        // Eye
        Eye_Left_Blink,
        Eye_Left_Wide,
        Eye_Left_Right,
        Eye_Left_Left,
        Eye_Left_Up,
        Eye_Left_Down,
        Eye_Right_Blink,
        Eye_Right_Wide,
        Eye_Right_Right,
        Eye_Right_Left,
        Eye_Right_Up,
        Eye_Right_Down,
        Eye_Left_Frown,
        Eye_Right_Frown,
        Eye_Left_Squeeze,
        Eye_Right_Squeeze,
        // TODO: Add pupil data

        // Gaze (Computed)
        Gaze_Left_Vertical,
        Gaze_Left_Horizontal,
        Gaze_Right_Vertical,
        Gaze_Right_Horizontal,
        Gaze_Vertical, // not the combined data by SRanipal
        Gaze_Horizontal,

        // Computed Eye
        Eye_Blink,
        Eye_Wide,
        Eye_Right,
        Eye_Left,
        Eye_Up,
        Eye_Down,
        Eye_Frown, // Originally in EyeShape_v2 but the actual data are separeted with the both eyes
        Eye_Squeeze,

        // Facial
        Jaw_Right,
        Jaw_Left,
        Jaw_Forward,
        Jaw_Open,
        Mouth_Ape_Shape,
        Mouth_Upper_Right,
        Mouth_Upper_Left,
        Mouth_Lower_Right,
        Mouth_Lower_Left,
        Mouth_Upper_Overturn,
        Mouth_Lower_Overturn,
        Mouth_Pout,
        Mouth_Smile_Right,
        Mouth_Smile_Left,
        Mouth_Sad_Right,
        Mouth_Sad_Left,
        Cheek_Puff_Right,
        Cheek_Puff_Left,
        Cheek_Suck,
        Mouth_Upper_UpRight,
        Mouth_Upper_UpLeft,
        Mouth_Lower_DownRight,
        Mouth_Lower_DownLeft,
        Mouth_Upper_Inside,
        Mouth_Lower_Inside,
        Mouth_Lower_Overlay,
        Tongue_LongStep1,
        Tongue_LongStep2,
        Tongue_Down,
        Tongue_Up,
        Tongue_Right,
        Tongue_Left,
        Tongue_Roll,
        Tongue_UpLeft_Morph,
        Tongue_UpRight_Morph,
        Tongue_DownLeft_Morph,
        Tongue_DownRight_Morph,

        // Calculated
        Jaw_Left_Right,
        Mouth_Sad_Smile_Right,
        Mouth_Sad_Smile_Left,
        Mouth_Smile,
        Mouth_Sad,
        Mouth_Sad_Smile,
        Mouth_Upper_Left_Right,
        Mouth_Lower_Left_Right,
        Mouth_Left_Right,
        Mouth_Upper_Inside_Overturn,
        Mouth_Lower_Inside_Overturn,
        Cheek_Puff,
        Cheek_Suck_Puff,
        Mouth_Upper_Up,
        Mouth_Lower_Down,
        Tongue_Left_Right,
        Tongue_Down_Up,


        Mouth_Left,
        Mouth_Right,
    }

    public enum DataType
    {
        Eye,
        Gaze,
        ComputedEye,
        Facial,
        ComputedFacial,
    }

    public class FaceKeyUtils
    {
        public static DataType GetDataType(FaceKey key)
        {
            switch (key)
            {
                case FaceKey.Eye_Left_Blink:
                case FaceKey.Eye_Left_Wide:
                case FaceKey.Eye_Left_Right:
                case FaceKey.Eye_Left_Left:
                case FaceKey.Eye_Left_Up:
                case FaceKey.Eye_Left_Down:
                case FaceKey.Eye_Right_Blink:
                case FaceKey.Eye_Right_Wide:
                case FaceKey.Eye_Right_Right:
                case FaceKey.Eye_Right_Left:
                case FaceKey.Eye_Right_Up:
                case FaceKey.Eye_Right_Down:
                case FaceKey.Eye_Left_Frown:
                case FaceKey.Eye_Right_Frown:
                case FaceKey.Eye_Left_Squeeze:
                case FaceKey.Eye_Right_Squeeze:
                    return DataType.Eye;

                case FaceKey.Gaze_Left_Vertical:
                case FaceKey.Gaze_Left_Horizontal:
                case FaceKey.Gaze_Right_Vertical:
                case FaceKey.Gaze_Right_Horizontal:
                case FaceKey.Gaze_Vertical:
                case FaceKey.Gaze_Horizontal:
                    return DataType.Gaze;

                case FaceKey.Eye_Blink:
                case FaceKey.Eye_Wide:
                case FaceKey.Eye_Right:
                case FaceKey.Eye_Left:
                case FaceKey.Eye_Up:
                case FaceKey.Eye_Down:
                case FaceKey.Eye_Frown:
                case FaceKey.Eye_Squeeze:
                    return DataType.ComputedEye;

                case FaceKey.Jaw_Right:
                case FaceKey.Jaw_Left:
                case FaceKey.Jaw_Forward:
                case FaceKey.Jaw_Open:
                case FaceKey.Mouth_Ape_Shape:
                case FaceKey.Mouth_Upper_Right:
                case FaceKey.Mouth_Upper_Left:
                case FaceKey.Mouth_Lower_Right:
                case FaceKey.Mouth_Lower_Left:
                case FaceKey.Mouth_Upper_Overturn:
                case FaceKey.Mouth_Lower_Overturn:
                case FaceKey.Mouth_Pout:
                case FaceKey.Mouth_Smile_Right:
                case FaceKey.Mouth_Smile_Left:
                case FaceKey.Mouth_Sad_Right:
                case FaceKey.Mouth_Sad_Left:
                case FaceKey.Cheek_Puff_Right:
                case FaceKey.Cheek_Puff_Left:
                case FaceKey.Cheek_Suck:
                case FaceKey.Mouth_Upper_UpRight:
                case FaceKey.Mouth_Upper_UpLeft:
                case FaceKey.Mouth_Lower_DownRight:
                case FaceKey.Mouth_Lower_DownLeft:
                case FaceKey.Mouth_Upper_Inside:
                case FaceKey.Mouth_Lower_Inside:
                case FaceKey.Mouth_Lower_Overlay:
                case FaceKey.Tongue_LongStep1:
                case FaceKey.Tongue_LongStep2:
                case FaceKey.Tongue_Down:
                case FaceKey.Tongue_Up:
                case FaceKey.Tongue_Right:
                case FaceKey.Tongue_Left:
                case FaceKey.Tongue_Roll:
                case FaceKey.Tongue_UpLeft_Morph:
                case FaceKey.Tongue_UpRight_Morph:
                case FaceKey.Tongue_DownLeft_Morph:
                case FaceKey.Tongue_DownRight_Morph:
                    return DataType.Facial;

                default:
                    return DataType.ComputedFacial;
            }
        }
        public static bool IsEssential(FaceKey key)
        {
            switch (key)
            {
                case FaceKey.Eye_Left_Blink:
                case FaceKey.Eye_Right_Blink:
                case FaceKey.Gaze_Horizontal:
                case FaceKey.Gaze_Vertical:
                case FaceKey.Jaw_Left_Right:
                case FaceKey.Jaw_Open:
                case FaceKey.Mouth_Pout:
                case FaceKey.Mouth_Smile:
                case FaceKey.Mouth_Sad:
                case FaceKey.Mouth_Sad_Smile:
                case FaceKey.Mouth_Upper_Left_Right:
                case FaceKey.Mouth_Lower_Left_Right:
                case FaceKey.Mouth_Left_Right:
                case FaceKey.Mouth_Upper_Inside_Overturn:
                case FaceKey.Mouth_Lower_Inside_Overturn:
                case FaceKey.Cheek_Puff:
                case FaceKey.Cheek_Suck:
                case FaceKey.Mouth_Upper_Up:
                case FaceKey.Mouth_Lower_Down:
                case FaceKey.Tongue_LongStep1:
                case FaceKey.Tongue_Left_Right:
                case FaceKey.Tongue_Down_Up:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsBipolar(FaceKey key)
        {
            switch (key)
            {
                case FaceKey.Jaw_Left_Right:
                case FaceKey.Mouth_Sad_Smile_Right:
                case FaceKey.Mouth_Sad_Smile_Left:
                case FaceKey.Mouth_Sad_Smile:
                case FaceKey.Mouth_Upper_Left_Right:
                case FaceKey.Mouth_Lower_Left_Right:
                case FaceKey.Mouth_Left_Right:
                case FaceKey.Mouth_Upper_Inside_Overturn:
                case FaceKey.Mouth_Lower_Inside_Overturn:
                case FaceKey.Cheek_Suck_Puff:
                case FaceKey.Tongue_Left_Right:
                case FaceKey.Tongue_Down_Up:
                    return true;
                default:
                    return false;
            }
        }
        public static float DefaultValue(FaceKey key)
        {
            switch (key)
            {
                case FaceKey.Eye_Left_Blink:
                case FaceKey.Eye_Left_Wide:
                case FaceKey.Eye_Left_Right:
                case FaceKey.Eye_Left_Left:
                case FaceKey.Eye_Left_Up:
                case FaceKey.Eye_Left_Down:
                case FaceKey.Eye_Right_Blink:
                case FaceKey.Eye_Right_Wide:
                case FaceKey.Eye_Right_Right:
                case FaceKey.Eye_Right_Left:
                case FaceKey.Eye_Right_Up:
                case FaceKey.Eye_Right_Down:
                case FaceKey.Eye_Left_Frown:
                case FaceKey.Eye_Right_Frown:
                case FaceKey.Eye_Left_Squeeze:
                case FaceKey.Eye_Right_Squeeze:
                    return 0;

                case FaceKey.Gaze_Left_Vertical:
                case FaceKey.Gaze_Left_Horizontal:
                case FaceKey.Gaze_Right_Vertical:
                case FaceKey.Gaze_Right_Horizontal:
                case FaceKey.Gaze_Vertical:
                case FaceKey.Gaze_Horizontal:
                    return .5f;

                case FaceKey.Eye_Blink:
                case FaceKey.Eye_Wide:
                case FaceKey.Eye_Right:
                case FaceKey.Eye_Left:
                case FaceKey.Eye_Up:
                case FaceKey.Eye_Down:
                case FaceKey.Eye_Frown:
                case FaceKey.Eye_Squeeze:
                    return 0;

                case FaceKey.Jaw_Right:
                case FaceKey.Jaw_Left:
                case FaceKey.Jaw_Forward:
                case FaceKey.Jaw_Open:
                case FaceKey.Mouth_Ape_Shape:
                case FaceKey.Mouth_Upper_Right:
                case FaceKey.Mouth_Upper_Left:
                case FaceKey.Mouth_Lower_Right:
                case FaceKey.Mouth_Lower_Left:
                case FaceKey.Mouth_Upper_Overturn:
                case FaceKey.Mouth_Lower_Overturn:
                case FaceKey.Mouth_Pout:
                case FaceKey.Mouth_Smile_Right:
                case FaceKey.Mouth_Smile_Left:
                case FaceKey.Mouth_Sad_Right:
                case FaceKey.Mouth_Sad_Left:
                case FaceKey.Cheek_Puff_Right:
                case FaceKey.Cheek_Puff_Left:
                case FaceKey.Cheek_Suck:
                case FaceKey.Mouth_Upper_UpRight:
                case FaceKey.Mouth_Upper_UpLeft:
                case FaceKey.Mouth_Lower_DownRight:
                case FaceKey.Mouth_Lower_DownLeft:
                case FaceKey.Mouth_Upper_Inside:
                case FaceKey.Mouth_Lower_Inside:
                case FaceKey.Mouth_Lower_Overlay:
                case FaceKey.Tongue_LongStep1:
                case FaceKey.Tongue_LongStep2:
                case FaceKey.Tongue_Down:
                case FaceKey.Tongue_Up:
                case FaceKey.Tongue_Right:
                case FaceKey.Tongue_Left:
                case FaceKey.Tongue_Roll:
                case FaceKey.Tongue_UpLeft_Morph:
                case FaceKey.Tongue_UpRight_Morph:
                case FaceKey.Tongue_DownLeft_Morph:
                case FaceKey.Tongue_DownRight_Morph:
                    return 0;
                case FaceKey.Jaw_Left_Right:
                case FaceKey.Mouth_Sad_Smile_Right:
                case FaceKey.Mouth_Sad_Smile_Left:
                    return .5f;
                case FaceKey.Mouth_Smile:
                case FaceKey.Mouth_Sad:
                    return 0;
                case FaceKey.Mouth_Sad_Smile:
                    return .5f;
                case FaceKey.Mouth_Upper_Left_Right:
                case FaceKey.Mouth_Lower_Left_Right:
                case FaceKey.Mouth_Left_Right:
                case FaceKey.Mouth_Upper_Inside_Overturn:
                case FaceKey.Mouth_Lower_Inside_Overturn:
                    return .5f;
                case FaceKey.Cheek_Puff:
                    return 0;
                case FaceKey.Cheek_Suck_Puff:
                    return .5f;
                case FaceKey.Mouth_Upper_Up:
                case FaceKey.Mouth_Lower_Down:
                    return 0;
                case FaceKey.Tongue_Left_Right:
                case FaceKey.Tongue_Down_Up:
                    return .5f;
            }
            return 0;
        }
    }
}
