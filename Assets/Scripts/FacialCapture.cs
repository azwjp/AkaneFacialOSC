using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using System.Linq;
using System;

public class FacialCapture : MonoBehaviour
{
    public OSC osc;
    Dictionary<LipShape_v2, float> lipWeight = new Dictionary<LipShape_v2, float>();
    [SerializeField]
    string addressRoot = "/avatar/parameters/";
    [SerializeField]
    string prefix = "facial_";

    [Header("ViveSR Parameter Mapping")]
    [SerializeField] string Jaw_Right = "Jaw_Right";
    [SerializeField] string Jaw_Left = "Jaw_Left";
    [SerializeField] string Jaw_Forward = "Jaw_Forward";
    [SerializeField] string Jaw_Open = "Jaw_Open";
    [SerializeField] string Mouth_Ape_Shape = "Mouth_Ape_Shape";
    [SerializeField] string Mouth_Upper_Right = "Mouth_Upper_Right";
    [SerializeField] string Mouth_Upper_Left = "Mouth_Upper_Left";
    [SerializeField] string Mouth_Lower_Right = "Mouth_Lower_Right";
    [SerializeField] string Mouth_Lower_Left = "Mouth_Lower_Left";
    [SerializeField] string Mouth_Upper_Overturn = "Mouth_Upper_Overturn";
    [SerializeField] string Mouth_Lower_Overturn = "Mouth_Lower_Overturn";
    [SerializeField] string Mouth_Pout = "Mouth_Pout";
    [SerializeField] string Mouth_Smile_Right = "Mouth_Smile_Right";
    [SerializeField] string Mouth_Smile_Left = "Mouth_Smile_Left";
    [SerializeField] string Mouth_Sad_Right = "Mouth_Sad_Right";
    [SerializeField] string Mouth_Sad_Left = "Mouth_Sad_Left";
    [SerializeField] string Cheek_Puff_Right = "Cheek_Puff_Right";
    [SerializeField] string Cheek_Puff_Left = "Cheek_Puff_Left";
    [SerializeField] string Cheek_Suck = "Cheek_Suck";
    [SerializeField] string Mouth_Upper_UpRight = "Mouth_Upper_UpRight";
    [SerializeField] string Mouth_Upper_UpLeft = "Mouth_Upper_UpLeft";
    [SerializeField] string Mouth_Lower_DownRight = "Mouth_Lower_DownRight";
    [SerializeField] string Mouth_Lower_DownLeft = "Mouth_Lower_DownLeft";
    [SerializeField] string Mouth_Upper_Inside = "Mouth_Upper_Inside";
    [SerializeField] string Mouth_Lower_Inside = "Mouth_Lower_Inside";
    [SerializeField] string Mouth_Lower_Overlay = "Mouth_Lower_Overlay";
    [SerializeField] string Tongue_LongStep1 = "Tongue_LongStep1";
    [SerializeField] string Tongue_LongStep2 = "Tongue_LongStep2";
    [SerializeField] string Tongue_Down = "Tongue_Down";
    [SerializeField] string Tongue_Up = "Tongue_Up";
    [SerializeField] string Tongue_Right = "Tongue_Right";
    [SerializeField] string Tongue_Left = "Tongue_Left";
    [SerializeField] string Tongue_Roll = "Tongue_Roll";
    [SerializeField] string Tongue_UpLeft_Morph = "Tongue_UpLeft_Morph";
    [SerializeField] string Tongue_UpRight_Morph = "Tongue_UpRight_Morph";
    [SerializeField] string Tongue_DownLeft_Morph = "Tongue_DownLeft_Morph";
    [SerializeField] string Tongue_DownRight_Morph = "Tongue_DownRight_Morph";

    [Header("Additional Parameter Mapping")]
    [SerializeField] string Mouth_Smile_Sad_Right = "Mouth_Smile_Sad_Right";
    [SerializeField] string Mouth_Smile_Sad_Left = "Mouth_Smile_Sad_Left";
    [SerializeField] string Mouth_Smile = "Mouth_Smile";
    [SerializeField] string Mouth_Sad = "Mouth_Sad";

    void Start()
    {
        if (!SRanipal_Lip_Framework.Instance.EnableLip)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return;
        SRanipal_Lip_v2.GetLipWeightings(out lipWeight);

        foreach (var param in lipWeight)
        {
            if (param.Key != LipShape_v2.Jaw_Open) continue;
            var key = GetMappedKey(param.Key);
            
            if (string.IsNullOrWhiteSpace(key)) continue;

            Send(key, param.Value);
        }

        SendBipolar(Mouth_Smile_Sad_Right, lipWeight[LipShape_v2.Mouth_Smile_Right], lipWeight[LipShape_v2.Mouth_Sad_Right]);
        SendBipolar(Mouth_Smile_Sad_Left, lipWeight[LipShape_v2.Mouth_Smile_Left], lipWeight[LipShape_v2.Mouth_Sad_Left]);
        SendAverage(Mouth_Smile, lipWeight[LipShape_v2.Mouth_Smile_Right], lipWeight[LipShape_v2.Mouth_Smile_Left]);
        SendAverage(Mouth_Sad, lipWeight[LipShape_v2.Mouth_Sad_Right], lipWeight[LipShape_v2.Mouth_Sad_Left]);

    }

    void Send(string address, float value)
    {
        var message = new OscMessage();
        message.address = addressRoot + prefix + address;
        message.values.Add(value);
        osc.Send(message);
    }

    void SendBipolar(string address, float upperValue, float lowerValue)
    {
        Send(
            address,
            upperValue > lowerValue ? upperValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
        );
    }
    void SendAverage(string address, float value1, float value2)
    {
        Send(
            address,
            (value1 + value2) / 2.0f
        );
    }

    string GetMappedKey(LipShape_v2 key)
    {
        switch(key)
        {
            case LipShape_v2.Jaw_Right:
                return Jaw_Right;
            case LipShape_v2.Jaw_Left:
                return Jaw_Left;
            case LipShape_v2.Jaw_Forward:
                return Jaw_Forward;
            case LipShape_v2.Jaw_Open:
                return Jaw_Open;
            case LipShape_v2.Mouth_Ape_Shape:
                return Mouth_Ape_Shape;
            case LipShape_v2.Mouth_Upper_Right:
                return Mouth_Upper_Right;
            case LipShape_v2.Mouth_Upper_Left:
                return Mouth_Upper_Left;
            case LipShape_v2.Mouth_Lower_Right:
                return Mouth_Lower_Right;
            case LipShape_v2.Mouth_Lower_Left:
                return Mouth_Lower_Left;
            case LipShape_v2.Mouth_Upper_Overturn:
                return Mouth_Upper_Overturn;
            case LipShape_v2.Mouth_Lower_Overturn:
                return Mouth_Lower_Overturn;
            case LipShape_v2.Mouth_Pout:
                return Mouth_Pout;
            case LipShape_v2.Mouth_Smile_Right:
                return Mouth_Smile_Right;
            case LipShape_v2.Mouth_Smile_Left:
                return Mouth_Smile_Left;
            case LipShape_v2.Mouth_Sad_Right:
                return Mouth_Sad_Right;
            case LipShape_v2.Mouth_Sad_Left:
                return Mouth_Sad_Left;
            case LipShape_v2.Cheek_Puff_Right:
                return Cheek_Puff_Right;
            case LipShape_v2.Cheek_Puff_Left:
                return Cheek_Puff_Left;
            case LipShape_v2.Cheek_Suck:
                return Cheek_Suck;
            case LipShape_v2.Mouth_Upper_UpRight:
                return Mouth_Upper_UpRight;
            case LipShape_v2.Mouth_Upper_UpLeft:
                return Mouth_Upper_UpLeft;
            case LipShape_v2.Mouth_Lower_DownRight:
                return Mouth_Lower_DownRight;
            case LipShape_v2.Mouth_Lower_DownLeft:
                return Mouth_Lower_DownLeft;
            case LipShape_v2.Mouth_Upper_Inside:
                return Mouth_Upper_Inside;
            case LipShape_v2.Mouth_Lower_Inside:
                return Mouth_Lower_Inside;
            case LipShape_v2.Mouth_Lower_Overlay:
                return Mouth_Lower_Overlay;
            case LipShape_v2.Tongue_LongStep1:
                return Tongue_LongStep1;
            case LipShape_v2.Tongue_LongStep2:
                return Tongue_LongStep2;
            case LipShape_v2.Tongue_Down:
                return Tongue_Down;
            case LipShape_v2.Tongue_Up:
                return Tongue_Up;
            case LipShape_v2.Tongue_Right:
                return Tongue_Right;
            case LipShape_v2.Tongue_Left:
                return Tongue_Left;
            case LipShape_v2.Tongue_Roll:
                return Tongue_Roll;
            case LipShape_v2.Tongue_UpLeft_Morph:
                return Tongue_UpLeft_Morph;
            case LipShape_v2.Tongue_UpRight_Morph:
                return Tongue_UpRight_Morph;
            case LipShape_v2.Tongue_DownLeft_Morph:
                return Tongue_DownLeft_Morph;
            case LipShape_v2.Tongue_DownRight_Morph:
                return Tongue_DownRight_Morph;
            default:
                return key.ToString();
        }
    }

    [Serializable]
    class VSRKeyMapper
    {
        public LipShape_v2 lipShape;
        public string target;
    }
}
