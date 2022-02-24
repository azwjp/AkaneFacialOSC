using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;

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

        Dictionary<FaceKey, FaceDataPreferences> prefs = new Dictionary<FaceKey, FaceDataPreferences>();
        ValueRowsUI vr;
        bool isDebug = false;
        const string PREF_FILE_PATH = "./preferences.json";
        public float maxAngle = 45f;


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
            prefs = Load();

            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                if (!prefs.ContainsKey(key))
                {
                    var val = new FaceDataPreferences();
                    prefs.Add(key, val);
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
            var rightRot = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
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
            foreach (var param in lipWeight)
            {
                var stringKey = param.Key.ToString();
                var key = (FaceKey)Enum.Parse(typeof(FaceKey), stringKey);
                var value = param.Value;

                Send(stringKey, key, value);
            }

            // Computed Data
            SendBipolar(FaceKey.Mouth_Smile_Sad_Right, lipWeight[LipShape_v2.Mouth_Smile_Right], lipWeight[LipShape_v2.Mouth_Sad_Right]);
            SendBipolar(FaceKey.Mouth_Smile_Sad_Left, lipWeight[LipShape_v2.Mouth_Smile_Left], lipWeight[LipShape_v2.Mouth_Sad_Left]);
            SendAverage(FaceKey.Mouth_Smile, lipWeight[LipShape_v2.Mouth_Smile_Right], lipWeight[LipShape_v2.Mouth_Smile_Left]);
            SendAverage(FaceKey.Mouth_Sad, lipWeight[LipShape_v2.Mouth_Sad_Right], lipWeight[LipShape_v2.Mouth_Sad_Left]);
        }

        void Send(string stringKey, FaceKey key, float value)
        {
            if (prefs.ContainsKey(key))
            {
                var pref = prefs[key];
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

        void SendBipolar(FaceKey key, float upperValue, float lowerValue)
        {
            Send(
                key,
                upperValue > lowerValue ? upperValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
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
            var pref = prefs[key];
            if (pref == null)
            {
                pref = new FaceDataPreferences();
                prefs.Add(key, pref);
            }
            pref.isSending = value.IsSending();
            pref.gain = value.GetGain();
            isDirty = true;
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(prefs);
            File.WriteAllText(PREF_FILE_PATH, json);
            isDirty = false;
        }

        Dictionary<FaceKey, FaceDataPreferences> Load()
        {
            try
            {
                var json = File.ReadAllText(PREF_FILE_PATH);
                var pref = JsonUtility.FromJson<Dictionary<FaceKey, FaceDataPreferences>>(json);
                foreach(var e in pref)
                {
                    uiRows.SetGain(e.Key, e.Value.gain);
                }
                return pref;
            }
            catch (Exception e)
            {
                return Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().ToDictionary(key => key, key => new FaceDataPreferences());
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

        [Serializable]
        class VSRKeyMapper
        {
            public LipShape_v2 lipShape;
            public string target;
        }

        [System.Serializable]
        class FaceDataPreferences
        {
            public bool isSending = true;
            public float gain = 1;
        }
    }
}
