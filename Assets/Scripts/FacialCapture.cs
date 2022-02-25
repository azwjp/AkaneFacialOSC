using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;
using System.Text;

namespace AZW.FaceOSC
{
    [RequireComponent(typeof(ValueRowsUI))]
    public class FacialCapture : MonoBehaviour
    {
        [Header("OSC")]
        public OSC osc;
        [SerializeField]
        string addressRoot = "/avatar/parameters/";

        [Header("UI Components")]
        [SerializeField] ValueRowsUI uiRows;
        [SerializeField] SaveButton saveButton;
        [SerializeField] Toggle debugToggle;


        Dictionary<LipShape_v2, float> lipWeight = new Dictionary<LipShape_v2, float>();
        //Dictionary<EyeShape_v2, float> eyeWeight = new Dictionary<EyeShape_v2, float>();
        static EyeData_v2 eyeData = new EyeData_v2();
        bool isUsingEyeCallback = false;

        Dictionary<FaceKey, FaceDataPreferences> facePrefs = new Dictionary<FaceKey, FaceDataPreferences>();
        ValueRowsUI vr;
        bool isDebug = false;
        const string PREF_FILE_PATH = "/preferences.json";
        public float maxAngle = Mathf.PI / 4.0f;


        bool _isDirty = false;
        public bool isDirty
        {
            get { return _isDirty; }
            private set
            {
                _isDirty = value;
                saveButton.SetDirty(value);
            }
        }

        void Start()
        {
            facePrefs = Load();

            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                if (!facePrefs.ContainsKey(key))
                {
                    var val = new FaceDataPreferences(key);
                    facePrefs.Add(key, val);
                    uiRows.SetGain(key, val.gain);
                }
            }

            if (!SRanipal_Lip_Framework.Instance.EnableLip)
            {
                enabled = false;
                return;
            }
        }

        void Update()
        {
            if (GetFacialData())
            {
                SendFacialData();
            }

            if (GetEyeData())
            {
                SendEyeData();
            }
        }

        bool GetEyeData()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return false;

            if (!isUsingEyeCallback)
            {
                if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback)
                {
                    SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                    isUsingEyeCallback = true;
                }
                else
                {
                    var error = SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
                    if (error != ViveSR.Error.WORK) return false;
                }
            }

            return eyeData.no_user;
        }

        void SendEyeData()
        {
            // Get the current value
            var leftEye = eyeData.verbose_data.left;
            var rightEye = eyeData.verbose_data.right;
            var leftExpression = eyeData.expression_data.left;
            var rightExpression = eyeData.expression_data.right;

            // Gaze
            // Eye balls usually don't rotate by z-axis
            // If invalid data such as closed eyes, return the centre
            var leftRot = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                ? new Vector2(
                    Mathf.Acos(Vector3.Dot(leftEye.gaze_direction_normalized, Vector3.right)),
                    Mathf.Acos(Vector3.Dot(leftEye.gaze_direction_normalized, Vector3.forward))
                )
                : Vector2.zero;
            var rightRot = rightEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                ? new Vector2(
                    Mathf.Acos(Vector3.Dot(rightEye.gaze_direction_normalized, Vector3.right)),
                    Mathf.Acos(Vector3.Dot(rightEye.gaze_direction_normalized, Vector3.forward))
                )
                : Vector2.zero;

            // Pupil
            // the same value with SRanipal_Eye_v2.GetPupilPosition
            // If invalid data such as closed eyes, return the centre
            var leftPupil = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY)
                ? new Vector2(
                    leftEye.pupil_position_in_sensor_area.x * 2 - 1,
                    leftEye.pupil_position_in_sensor_area.y * -2 + 1
                )
                : Vector2.zero;
            var rightPupil = rightEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY)
                ? new Vector2(
                    rightEye.pupil_position_in_sensor_area.x * 2 - 1,
                    rightEye.pupil_position_in_sensor_area.y * -2 + 1
                )
                : Vector2.zero;

            // Openness
            // If invalid data such as closed eyes, return 0 which means a closed eye
            var leftOpenness = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) ? leftEye.eye_openness : 0;
            var rightOpenness = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) ? rightEye.eye_openness : 0;

            SendRotation(FaceKey.Gaze_Left_Horizontal, leftRot.y);
            SendRotation(FaceKey.Gaze_Left_Vertical, leftRot.x);
            SendRotation(FaceKey.Gaze_Right_Horizontal, rightRot.y);
            SendRotation(FaceKey.Gaze_Right_Vertical, rightRot.x);
            SendAverageRotation(FaceKey.Gaze_Horizontal, leftRot.y, rightRot.y);
            SendAverageRotation(FaceKey.Gaze_Vertical, leftRot.x, rightRot.x);

            // Weight
            // the same value with SRanipal_Eye_v2.GetEyeWeightings
            SendWithAverage(FaceKey.Eye_Left_Up, FaceKey.Eye_Right_Up, FaceKey.Eye_Up, leftExpression.eye_wide, rightExpression.eye_wide);
            SendWithAverage(FaceKey.Eye_Left_Down, FaceKey.Eye_Right_Down, FaceKey.Eye_Down, leftRot.y < 0 ? leftRot.y : 0, rightRot.y < 0 ? rightRot.y : 0);
            SendWithAverage(FaceKey.Eye_Left_Left, FaceKey.Eye_Right_Left, FaceKey.Eye_Left, Mathf.Max(-leftPupil.x, 0f), Mathf.Max(-rightPupil.x, 0f));
            SendWithAverage(FaceKey.Eye_Left_Right, FaceKey.Eye_Right_Right, FaceKey.Eye_Right, Mathf.Max(leftPupil.x, 0f), Mathf.Max(rightPupil.x, 0f));
            SendWithAverage(FaceKey.Eye_Left_Blink, FaceKey.Eye_Right_Blink, FaceKey.Eye_Blink, 1 - leftOpenness, 1 - rightOpenness);
            SendWithAverage(FaceKey.Eye_Left_Wide, FaceKey.Eye_Right_Wide, FaceKey.Eye_Wide, leftExpression.eye_wide, rightExpression.eye_wide); // The same with Eye_Left_Up
            SendWithAverage(FaceKey.Eye_Left_Frown, FaceKey.Eye_Right_Frown, FaceKey.Eye_Frown, leftExpression.eye_frown, rightExpression.eye_frown);
            SendWithAverage(FaceKey.Eye_Left_Squeeze, FaceKey.Eye_Right_Squeeze, FaceKey.Eye_Squeeze, leftExpression.eye_squeeze, rightExpression.eye_squeeze);
        }

        bool GetFacialData()
        {
            if (isDebug)
            {
                lipWeight = Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>()
                    .Where(faceKey => KeyUtils.GetDataType(faceKey) == DataType.Facial)
                    .Select(faceKey => (LipShape_v2)Enum.Parse(typeof(LipShape_v2), faceKey.ToString()))
                    .ToDictionary(
                        e => e,
                        e => Mathf.Sin(Time.fixedTime) / 2.0f + 0.5f
                    );

                return true;
            }
            else
            {
                if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return false;
                SRanipal_Lip_v2.GetLipWeightings(out lipWeight);
                return true;
            }
        }

        void SendFacialData()
        {
            Send(FaceKey.Jaw_Right, lipWeight[LipShape_v2.Jaw_Right]);
            Send(FaceKey.Jaw_Left, lipWeight[LipShape_v2.Jaw_Left]);
            Send(FaceKey.Jaw_Forward, lipWeight[LipShape_v2.Jaw_Forward]);
            Send(FaceKey.Jaw_Open, lipWeight[LipShape_v2.Jaw_Open]);

            Send(FaceKey.Mouth_Upper_Right, lipWeight[LipShape_v2.Mouth_Upper_Right]);
            Send(FaceKey.Mouth_Upper_Left, lipWeight[LipShape_v2.Mouth_Upper_Left]);
            Send(FaceKey.Mouth_Lower_Right, lipWeight[LipShape_v2.Mouth_Lower_Right]);
            Send(FaceKey.Mouth_Lower_Left, lipWeight[LipShape_v2.Mouth_Lower_Left]);

            Send(FaceKey.Mouth_Ape_Shape, lipWeight[LipShape_v2.Mouth_Ape_Shape]);

            Send(FaceKey.Mouth_Upper_Overturn, lipWeight[LipShape_v2.Mouth_Upper_Overturn]);
            Send(FaceKey.Mouth_Lower_Overturn, lipWeight[LipShape_v2.Mouth_Lower_Overturn]);

            Send(FaceKey.Mouth_Pout, lipWeight[LipShape_v2.Mouth_Pout]);

            // Smile
            // Sad
            // Suck
            // Puff

            Send(FaceKey.Mouth_Upper_UpRight, lipWeight[LipShape_v2.Mouth_Upper_UpRight]);
            Send(FaceKey.Mouth_Upper_UpLeft, lipWeight[LipShape_v2.Mouth_Upper_UpLeft]);
            Send(FaceKey.Mouth_Lower_DownRight, lipWeight[LipShape_v2.Mouth_Lower_DownRight]);
            Send(FaceKey.Mouth_Lower_DownLeft, lipWeight[LipShape_v2.Mouth_Lower_DownLeft]);
            Send(FaceKey.Mouth_Upper_Inside, lipWeight[LipShape_v2.Mouth_Upper_Inside]);
            Send(FaceKey.Mouth_Lower_Inside, lipWeight[LipShape_v2.Mouth_Lower_Inside]);
            Send(FaceKey.Mouth_Lower_Overlay, lipWeight[LipShape_v2.Mouth_Lower_Overlay]);

            Send(FaceKey.Tongue_LongStep1, lipWeight[LipShape_v2.Tongue_LongStep1]);
            Send(FaceKey.Tongue_LongStep2, lipWeight[LipShape_v2.Tongue_LongStep2]);
            Send(FaceKey.Tongue_Down, lipWeight[LipShape_v2.Tongue_Down]);
            Send(FaceKey.Tongue_Up, lipWeight[LipShape_v2.Tongue_Up]);
            Send(FaceKey.Tongue_Right, lipWeight[LipShape_v2.Tongue_Right]);
            Send(FaceKey.Tongue_Left, lipWeight[LipShape_v2.Tongue_Left]);
            Send(FaceKey.Tongue_Roll, lipWeight[LipShape_v2.Tongue_Roll]);
            Send(FaceKey.Tongue_UpLeft_Morph, lipWeight[LipShape_v2.Tongue_UpLeft_Morph]);
            Send(FaceKey.Tongue_UpRight_Morph, lipWeight[LipShape_v2.Tongue_UpRight_Morph]);
            Send(FaceKey.Tongue_DownLeft_Morph, lipWeight[LipShape_v2.Tongue_DownLeft_Morph]);
            Send(FaceKey.Tongue_DownRight_Morph, lipWeight[LipShape_v2.Tongue_DownRight_Morph]);


            // Calculated
            SendWithAverageAndBipolar(
                FaceKey.Mouth_Sad_Left, FaceKey.Mouth_Sad_Right, FaceKey.Mouth_Smile_Left, FaceKey.Mouth_Smile_Right,
                FaceKey.Mouth_Sad_Smile_Left, FaceKey.Mouth_Sad_Smile_Right,
                FaceKey.Mouth_Sad, FaceKey.Mouth_Smile,
                FaceKey.Mouth_Sad_Smile,
                lipWeight[LipShape_v2.Mouth_Sad_Left], lipWeight[LipShape_v2.Mouth_Sad_Right], lipWeight[LipShape_v2.Mouth_Smile_Left], lipWeight[LipShape_v2.Mouth_Smile_Right]
            );

            SendBipolar(FaceKey.Mouth_Upper_Left_Right, lipWeight[LipShape_v2.Mouth_Upper_Left], lipWeight[LipShape_v2.Mouth_Upper_Right]);
            SendBipolar(FaceKey.Mouth_Lower_Left_Right, lipWeight[LipShape_v2.Mouth_Lower_Left], lipWeight[LipShape_v2.Mouth_Lower_Right]);
            SendAverageAndBipolar(FaceKey.Mouth_Left_Right, lipWeight[LipShape_v2.Mouth_Lower_Left], lipWeight[LipShape_v2.Mouth_Lower_Left], lipWeight[LipShape_v2.Mouth_Lower_Right], lipWeight[LipShape_v2.Mouth_Lower_Right]);

            Send(FaceKey.Cheek_Puff_Right, lipWeight[LipShape_v2.Cheek_Puff_Right]);
            Send(FaceKey.Cheek_Puff_Left, lipWeight[LipShape_v2.Cheek_Puff_Left]);
            Send(FaceKey.Cheek_Suck, lipWeight[LipShape_v2.Cheek_Suck]);
            var cheekPuff = (lipWeight[LipShape_v2.Cheek_Puff_Left] + lipWeight[LipShape_v2.Cheek_Puff_Right]) / 2.0f;
            Send(FaceKey.Cheek_Puff, cheekPuff);
            SendBipolar(FaceKey.Cheek_Suck_Puff, lipWeight[LipShape_v2.Cheek_Suck], cheekPuff);
        }

        void Send(string stringKey, FaceKey key, float value)
        {
            if (facePrefs.ContainsKey(key))
            {
                var pref = facePrefs[key];
                value *= pref.gain;
                uiRows.SetValue(key, value);
                if (!pref.isSending) return;
            }

            var message = new OscMessage();
            message.address = addressRoot + stringKey;
            message.values.Add(value);
            osc.Send(message);

        }

        void Send(FaceKey key, float value)
        {
            Send(key.ToString(), key, value);
        }

        void SendBipolar(FaceKey key, float lowerValue, float higherValue)
        {
            Send(
                key,
                higherValue > lowerValue ? higherValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
            );
        }
        void SendWithAverageAndBipolar(
            FaceKey lowerKey1, FaceKey lowerKey2, FaceKey higherKey1, FaceKey higherKey2,
            FaceKey key1, FaceKey key2, FaceKey lowerKey, FaceKey higherKey,
            FaceKey unitedKey,
            float lower1, float lower2, float higher1, float higher2)
        {
            var higherValue = (higher1 + higher2) / 2.0f;
            var lowerValue = (lower1 + lower2) / 2.0f;

            // Single values
            Send(higherKey1, higher1);
            Send(higherKey2, higher2);
            Send(lowerKey1, lower1);
            Send(lowerKey2, lower2);

            // Bipolar
            SendBipolar(key1, lower1, higher1);
            SendBipolar(key2, lower2, higher2);

            // Average
            Send(higherKey, higherValue);
            Send(lowerKey, lowerValue);

            // Bipolar with average
            Send(
                unitedKey,
                higherValue > lowerValue ? higherValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
            );
        }

        void SendAverageAndBipolar(FaceKey unitedKey, float lower1, float lower2, float higher1, float higher2)
        {
            var higherValue = (higher1 + higher2) / 2.0f;
            var lowerValue = (lower1 + lower2) / 2.0f;

            Send(
                unitedKey,
                higherValue > lowerValue ? higherValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
            );
        }

        void SendAverage(FaceKey key, float value1, float value2)
        {
            Send(
                key,
                (value1 + value2) / 2.0f
            );
        }

        void SendWithAverage(FaceKey key1, FaceKey key2, FaceKey keyAverage, float value1, float value2)
        {
            Send(key1, value1);
            Send(key2, value2);
            Send(
                keyAverage,
                (value1 + value2) / 2.0f
            );
        }

        void SendRotation(FaceKey key, float rotation)
        {
            if (rotation > maxAngle) rotation = maxAngle;
            else if (rotation < -maxAngle) rotation = -maxAngle;

            Send(key, rotation / maxAngle);
        }
        void SendAverageRotation(FaceKey key, float rotation1, float rotation2)
        {
            var rotation = (rotation1 - rotation2) / 2.0f;
            Send(key, rotation / maxAngle);
        }

        public void UpdatePreference(FaceKey key, FaceValueRow value)
        {
            var pref = facePrefs[key];
            if (pref == null)
            {
                pref = new FaceDataPreferences(key);
                facePrefs.Add(key, pref);
            }
            pref.isSending = value.IsSending();
            pref.gain = value.GetGain();
            isDirty = true;
        }

        public void Save()
        {
            var preferences = new Preferences();
            preferences.faceDataPreferences = facePrefs.Values.ToArray();
            var json = JsonUtility.ToJson(preferences, true);
            File.WriteAllText(Application.persistentDataPath + PREF_FILE_PATH, json, Encoding.UTF8);
            isDirty = false;
        }

        Dictionary<FaceKey, FaceDataPreferences> Load()
        {
            try
            {
                var json = File.ReadAllText(Application.persistentDataPath + PREF_FILE_PATH, Encoding.UTF8);
                var preferences = JsonUtility.FromJson<Preferences>(json);
                facePrefs = preferences.faceDataPreferences.ToDictionary(e => (FaceKey)Enum.Parse(typeof(FaceKey), e.key), e => new FaceDataPreferences(e.key, e.isSending, e.gain));
                foreach(var e in facePrefs)
                {
                    uiRows.SetGain(e.Key, e.Value.gain);
                }
                return facePrefs;
            }
            catch (Exception e)
            {
                return Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().ToDictionary(key => key, key => new FaceDataPreferences(key));
            }
        }

        public void ToggleDebug()
        {
            isDebug = debugToggle.isOn;
        }

        static void EyeCallback(ref EyeData_v2 eye_data)
        {
            eyeData = eye_data;
        }


    }
    [Serializable]
    public class VSRKeyMapper
    {
        public LipShape_v2 lipShape;
        public string target;
    }

    [Serializable]
    public class Preferences
    {
        [SerializeField] public FaceDataPreferences[] faceDataPreferences;
    }
    [Serializable]
    public class FaceDataPreferences
    {
        public string key;
        public bool isSending = true;
        public float gain = 1;

        public FaceDataPreferences(string key)
        {
            this.key = key;
        }
        public FaceDataPreferences(FaceKey key)
        {
            this.key = key.ToString();
        }
        public FaceDataPreferences(string key, bool isSending, float gain)
        {
            this.key = key;
            this.isSending = isSending;
            this.gain = gain;
        }
    }
}
