namespace AZW.FaceOSC
{
    public enum FaceKey
    {
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
        Mouth_Smile_Sad_Right,
        Mouth_Smile_Sad_Left,
        Mouth_Smile,
        Mouth_Sad,
    }

    public enum DataType
    {
        Facial,
        Calculated,
    }

    public class KeyUtils
    {
        public static DataType GetDataType(FaceKey key)
        {
            switch (key)
            {
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
                    return DataType.Calculated;
            }
        }
    }
}