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
using Pimax.EyeTracking;
using System.Threading.Tasks;

namespace AZW.FaceOSC
{
    [RequireComponent(typeof(ValueRowsUI))]
    public class FacialCapture : MonoBehaviour
    {
        const int VERSION = 1;

        [Header("OSC")]
        public OSC osc;
        [SerializeField]
        string addressRoot = "/avatar/parameters/";

        [Header("Tracking SDK")]
        [SerializeField] SRanipal_Eye_Framework eyeFramework;
        [SerializeField] SRanipal_Lip_Framework lipFramework;

        [Header("UI Components")]
        [SerializeField] ValueRowsUI uiRows;
        [SerializeField] SaveButton saveButton;
        [SerializeField] ToggleWithStatus useEyeTracking;
        [SerializeField] ToggleWithStatus useFacialTracking;
        [SerializeField] Toggle debugToggle;
        [SerializeField] Dropdown eyeTrackingTypeList;
        [SerializeField] Button calibrationButton;
        [SerializeField] InputField maxAngle;
        [SerializeField] I18nLangChanger language;


        Dictionary<LipShape_v2, float> lipWeight = new Dictionary<LipShape_v2, float>();
        //Dictionary<EyeShape_v2, float> eyeWeight = new Dictionary<EyeShape_v2, float>();
        static EyeData_v2 eyeData = new EyeData_v2();
        bool isUsingEyeCallback = false;

        Dictionary<FaceKey, FaceDataPreferences> facePrefs = new Dictionary<FaceKey, FaceDataPreferences>();
        const string PREF_FILE_PATH = "/preferences.json";
        public float maxAngleRadian = 45.0f / 180.0f * Mathf.PI;

        static readonly Vector2 halfVector = new Vector2(0.5f, 0.5f);
        // Pimax Asee
        EyeTracker pimaxAseeEye;
        long pimaxAseeLastUpdateTime = 0;
        Vector2 pimaxAseeCenterLeft = halfVector;
        Vector2 pimaxAseeCenterRight = halfVector;

        DeviceState enableFace = DeviceState.Disbled;
        DeviceState enableEye = DeviceState.Disbled;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051", Justification = "Used by Unity")]
        void Start()
        {
            eyeTrackingTypeList.options = Enum.GetValues(typeof(EyeTrackingType)).Cast<EyeTrackingType>().Select(x => new Dropdown.OptionData(x.ToString())).ToList();

            facePrefs = Load();
            isDirty = false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051", Justification = "Used by Unity")]
        void Update()
        {
            maxAngleRadian = int.Parse(maxAngle.text) / 180.0f * Mathf.PI;

            switch (eyeTrackingTypeList.value)
            {
                case (int)EyeTrackingType.ViveSRanipal:
                    if (SRanipal_Lip_Framework.Instance == null) useEyeTracking.SetState(DeviceState.Unavailable);
                    else
                    {
                        switch (SRanipal_Eye_Framework.Status)
                        {
                            case SRanipal_Eye_Framework.FrameworkStatus.STOP:
                                if (enableEye == DeviceState.Starting) useEyeTracking.SetState(DeviceState.Starting);
                                else useEyeTracking.SetState(DeviceState.Disbled);
                                break;
                            case SRanipal_Eye_Framework.FrameworkStatus.START:
                                useEyeTracking.SetState(DeviceState.Starting);
                                break;
                            case SRanipal_Eye_Framework.FrameworkStatus.WORKING:
                                useEyeTracking.SetState(DeviceState.Enabled);
                                break;
                            case SRanipal_Eye_Framework.FrameworkStatus.ERROR:
                            default:
                                useEyeTracking.SetState(DeviceState.Unavailable);
                                break;
                        }
                    }
                    break;
                case (int)EyeTrackingType.PimaxAsee:
                    if (pimaxAseeEye?.Active == true) useEyeTracking.SetState(DeviceState.Enabled);
                    else useEyeTracking.SetState(enableEye);
                    break;
            }

            if (SRanipal_Lip_Framework.Instance == null) useFacialTracking.SetState(DeviceState.Unavailable);
            else
            {
                switch (SRanipal_Lip_Framework.Status)
                {
                    case SRanipal_Lip_Framework.FrameworkStatus.STOP:
                        if (enableFace == DeviceState.Starting) useFacialTracking.SetState(DeviceState.Starting);
                        else useFacialTracking.SetState(DeviceState.Disbled);
                        break;
                    case SRanipal_Lip_Framework.FrameworkStatus.START:
                        useFacialTracking.SetState(DeviceState.Starting);
                        break;
                    case SRanipal_Lip_Framework.FrameworkStatus.WORKING:
                        useFacialTracking.SetState(DeviceState.Enabled);
                        break;
                    case SRanipal_Lip_Framework.FrameworkStatus.ERROR:
                    default:
                        useFacialTracking.SetState(DeviceState.Unavailable);
                        break;
                }
            }

            if (useFacialTracking.toggle.isOn && GetFacialData())
            {
                SendFacialData();
            }

            if (useEyeTracking.toggle.isOn)
            {
                switch (eyeTrackingTypeList.value)
                {
                    case (int)EyeTrackingType.ViveSRanipal:
                        if (GetSRanipalEyeData())  SendSRanipalEyeData();
                        break;
                    case (int)EyeTrackingType.PimaxAsee:
                        if (GetPimaxDroolonEyeData()) SendPimaxDroolonEyeData();
                        break;
                }
            }
        }

        async void SwitchFacialTracker(bool enable)
        {
            await Task.Run(() =>
            {
                enableFace = enable ? DeviceState.Starting : DeviceState.Stopping;
                lipFramework.EnableLip = enable;
                if (enable) lipFramework.StartFramework();
                else lipFramework.StopFramework();
                enableFace = enable ? DeviceState.Enabled : DeviceState.Disbled;
            });
        }

        public void OnFacialTrackerSwitched(bool isOn)
        {
            if (!isOn)
            {
                SwitchFacialTracker(false);
            }
        }

        public void OnEyeTrackerSwitched(bool isOn)
        {
            if (!isOn)
            {
                Release();
                SwitchEyeTracker((EyeTrackingType)eyeTrackingTypeList.value, false);
            }
        }

        async void SwitchEyeTracker(EyeTrackingType device, bool enable)
        {
            await Task.Run(() =>
            {
                enableEye = enable ? DeviceState.Starting : DeviceState.Stopping;
                switch (device)
                {
                    case EyeTrackingType.ViveSRanipal:
                        eyeFramework.EnableEye = enable;
                        if (enable) eyeFramework.StartFramework();
                        else eyeFramework.StopFramework();
                        break;
                    case EyeTrackingType.PimaxAsee:
                        if (pimaxAseeEye != null)
                        {
                            if (enable) pimaxAseeEye.Start();
                            else pimaxAseeEye.Stop();
                        }
                        break;
                    default: throw new NotImplementedException();

                }
                enableEye = enable ? DeviceState.Enabled : DeviceState.Disbled;
            });
        }

        bool GetSRanipalEyeData()
        {
            if (debugToggle.isOn)
            {
                if (enableEye == DeviceState.Enabled) SwitchEyeTracker(EyeTrackingType.ViveSRanipal, false);

                if (isUsingEyeCallback && SRanipal_Eye_Framework.Instance.EnableEyeDataCallback)
                {
                    SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                    isUsingEyeCallback = false;
                }

                var leftEye = eyeData.verbose_data.left;
                var rightEye = eyeData.verbose_data.right;
                var leftExpression = eyeData.expression_data.left;
                var rightExpression = eyeData.expression_data.right;
                leftEye.gaze_direction_normalized
                    = rightEye.gaze_direction_normalized
                    = new Vector3(Mathf.Cos(Time.fixedTime), Mathf.Sin(Time.fixedTime), 1).normalized;
                leftEye.pupil_position_in_sensor_area
                    = rightEye.pupil_position_in_sensor_area
                    = new Vector2(Mathf.Cos(Time.fixedTime), Mathf.Sin(Time.fixedTime));
                leftExpression.eye_frown
                    = rightExpression.eye_frown
                    = leftExpression.eye_wide
                    = rightExpression.eye_wide
                    = leftExpression.eye_squeeze
                    = rightExpression.eye_squeeze
                    = leftEye.eye_openness
                    = rightEye.eye_openness
                    = Mathf.Abs(Mathf.Sin(Time.fixedTime));
                leftEye.eye_data_validata_bit_mask
                    = rightEye.eye_data_validata_bit_mask
                    = ulong.MaxValue;
                eyeData.verbose_data.left = leftEye;
                eyeData.verbose_data.right = rightEye;
                eyeData.expression_data.left = leftExpression;
                eyeData.expression_data.right = rightExpression;
                return true;
            }
            else
            {
                if (useEyeTracking.toggle.isOn && enableEye == DeviceState.Disbled) SwitchEyeTracker(EyeTrackingType.ViveSRanipal, true);
                else if (!useEyeTracking.toggle.isOn && enableEye == DeviceState.Enabled) SwitchEyeTracker(EyeTrackingType.ViveSRanipal, false);

                if (enableEye != DeviceState.Enabled
                    || !eyeFramework.enabled
                    || SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING) return false;

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
        }

        bool GetPimaxDroolonEyeData()
        {
            if (pimaxAseeEye == null)
            {
                pimaxAseeEye = new EyeTracker();
                SwitchEyeTracker(EyeTrackingType.PimaxAsee, true);
                return false;
            }
            if (!pimaxAseeEye.Active)
            {
                if (enableEye != DeviceState.Starting) SwitchEyeTracker(EyeTrackingType.PimaxAsee, true);
                return false;
            }

            if (pimaxAseeLastUpdateTime == 0)
            {
                pimaxAseeLastUpdateTime = pimaxAseeEye.Timestamp;
                return false;
            }
            else if (pimaxAseeLastUpdateTime >= pimaxAseeEye.Timestamp)
            {
                return false;
            }
            else
            {
                pimaxAseeLastUpdateTime = pimaxAseeEye.Timestamp;
                return true;
            }

        }

        void Release()
        {
            if (isUsingEyeCallback)
            {
                SRanipal_Eye.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                isUsingEyeCallback = false;
                eyeFramework.StopFramework();
            }
            if (pimaxAseeEye != null && pimaxAseeEye.Active)
            {
                pimaxAseeEye.Stop();
            }
        }

        void OnDestroy()
        {
            Release();
            lipFramework.StopFramework();
            lipFramework.EnableLip = false;
            eyeFramework.EnableEye = false;
        }

        public void OnEyeTrackerChanged()
        {
            Release();
            switch((EyeTrackingType)eyeTrackingTypeList.value)
            {
                case EyeTrackingType.ViveSRanipal:
                    maxAngle.gameObject.SetActive(true);
                    calibrationButton.gameObject.SetActive(false);
                    break;
                case EyeTrackingType.PimaxAsee:
                    maxAngle.gameObject.SetActive(false);
                    calibrationButton.gameObject.SetActive(true);
                    break;
            }
        }

        public void ResetCenter()
        {
            pimaxAseeCenterLeft = pimaxAseeEye.LeftEye.Expression.PupilCenter;
            pimaxAseeCenterRight = pimaxAseeEye.RightEye.Expression.PupilCenter;
        }

        void SendSRanipalEyeData()
        {
            // Get the current value
            var leftEye = eyeData.verbose_data.left;
            var rightEye = eyeData.verbose_data.right;
            var leftExpression = eyeData.expression_data.left;
            var rightExpression = eyeData.expression_data.right;

            // Gaze
            // Eye balls usually don't rotate by z-axis
            // If invalid data such as closed eyes, return the centre
            // The data from tracler is right-handed coordinate system
            var leftRot = leftEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                ? new Vector2(
                    (leftEye.gaze_direction_normalized.x > 0 ? 1 : -1) * Mathf.Acos(Vector2.Dot(new Vector2(-leftEye.gaze_direction_normalized.x, leftEye.gaze_direction_normalized.z).normalized, Vector2.up)),
                    (leftEye.gaze_direction_normalized.y > 0 ? 1 : -1) * Mathf.Acos(Vector2.Dot(new Vector2(leftEye.gaze_direction_normalized.y, leftEye.gaze_direction_normalized.z).normalized, Vector2.up))
                )
                : Vector2.zero;
            var rightRot = rightEye.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                ? new Vector2(
                    (rightEye.gaze_direction_normalized.x > 0 ? 1 : -1) * Mathf.Acos(Vector2.Dot(new Vector2(-rightEye.gaze_direction_normalized.x, rightEye.gaze_direction_normalized.z).normalized, Vector2.up)),
                    (rightEye.gaze_direction_normalized.y > 0 ? 1 : -1) * Mathf.Acos(Vector2.Dot(new Vector2(rightEye.gaze_direction_normalized.y, rightEye.gaze_direction_normalized.z).normalized, Vector2.up))
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

            SendRotationWithAverage(FaceKey.Gaze_Left_Horizontal, FaceKey.Gaze_Right_Horizontal, FaceKey.Gaze_Horizontal, leftRot.x, rightRot.x);
            SendRotationWithAverage(FaceKey.Gaze_Left_Vertical, FaceKey.Gaze_Right_Vertical, FaceKey.Gaze_Vertical, leftRot.y, rightRot.y);

            // Weight
            // the same value with SRanipal_Eye_v2.GetEyeWeightings
            SendWithAverage(FaceKey.Eye_Left_Up, FaceKey.Eye_Right_Up, FaceKey.Eye_Up, leftExpression.eye_wide, rightExpression.eye_wide);
            SendWithAverage(FaceKey.Eye_Left_Down, FaceKey.Eye_Right_Down, FaceKey.Eye_Down, leftRot.y < 0 ? -leftRot.y : 0, rightRot.y < 0 ? -rightRot.y : 0);
            SendWithAverage(FaceKey.Eye_Left_Left, FaceKey.Eye_Right_Left, FaceKey.Eye_Left, Mathf.Max(-leftPupil.x, 0f), Mathf.Max(-rightPupil.x, 0f));
            SendWithAverage(FaceKey.Eye_Left_Right, FaceKey.Eye_Right_Right, FaceKey.Eye_Right, Mathf.Max(leftPupil.x, 0f), Mathf.Max(rightPupil.x, 0f));
            SendWithAverage(FaceKey.Eye_Left_Blink, FaceKey.Eye_Right_Blink, FaceKey.Eye_Blink, 1 - leftOpenness, 1 - rightOpenness);
            SendWithAverage(FaceKey.Eye_Left_Wide, FaceKey.Eye_Right_Wide, FaceKey.Eye_Wide, leftExpression.eye_wide, rightExpression.eye_wide); // The same with Eye_Left_Up
            SendWithAverage(FaceKey.Eye_Left_Frown, FaceKey.Eye_Right_Frown, FaceKey.Eye_Frown, leftExpression.eye_frown, rightExpression.eye_frown);
            SendWithAverage(FaceKey.Eye_Left_Squeeze, FaceKey.Eye_Right_Squeeze, FaceKey.Eye_Squeeze, leftExpression.eye_squeeze, rightExpression.eye_squeeze);
        }

        void SendPimaxDroolonEyeData()
        {
            var leftRot = (pimaxAseeEye.LeftEye.Expression.PupilCenter - pimaxAseeCenterLeft + halfVector);
            var rightRot = (pimaxAseeEye.RightEye.Expression.PupilCenter - pimaxAseeCenterRight + halfVector);

            SendWithAverage(FaceKey.Gaze_Left_Horizontal, FaceKey.Gaze_Right_Horizontal, FaceKey.Gaze_Horizontal, leftRot.x, rightRot.x);
            SendWithAverage(FaceKey.Gaze_Left_Vertical, FaceKey.Gaze_Right_Vertical, FaceKey.Gaze_Vertical, leftRot.y, rightRot.y);
            SendWithAverage(FaceKey.Eye_Left_Blink, FaceKey.Eye_Right_Blink, FaceKey.Eye_Blink, 1 - pimaxAseeEye.LeftEye.Expression.Openness, 1 - pimaxAseeEye.RightEye.Expression.Openness);
        }

        bool GetFacialData()
        {
            if (debugToggle.isOn)
            {
                if (enableFace == DeviceState.Enabled) SwitchFacialTracker(false);

                lipWeight = Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>()
                    .Where(faceKey => FaceKeyUtils.GetDataType(faceKey) == DataType.Facial)
                    .Select(faceKey => (LipShape_v2)Enum.Parse(typeof(LipShape_v2), faceKey.ToString()))
                    .ToDictionary(
                        e => e,
                        e => Mathf.Sin(Time.fixedTime) / 2.0f + 0.5f
                    );

                return true;
            }
            else
            {
                if (useFacialTracking.toggle.isOn && enableFace == DeviceState.Disbled) SwitchFacialTracker(true);
                else if (!useFacialTracking.toggle.isOn && enableFace == DeviceState.Enabled) SwitchFacialTracker(false);

                if (enableFace != DeviceState.Enabled
                    || !lipFramework.enabled
                    || SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return false;
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
            SendBipolar(FaceKey.Jaw_Left_Right, lipWeight[LipShape_v2.Jaw_Left], lipWeight[LipShape_v2.Jaw_Right]);
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


            SendBipolar(FaceKey.Mouth_Upper_Inside_Overturn, lipWeight[LipShape_v2.Mouth_Upper_Inside], lipWeight[LipShape_v2.Mouth_Upper_Overturn]);
            SendBipolar(FaceKey.Mouth_Lower_Inside_Overturn, lipWeight[LipShape_v2.Mouth_Lower_Inside], lipWeight[LipShape_v2.Mouth_Lower_Overturn]);

            Send(FaceKey.Cheek_Puff_Right, lipWeight[LipShape_v2.Cheek_Puff_Right]);
            Send(FaceKey.Cheek_Puff_Left, lipWeight[LipShape_v2.Cheek_Puff_Left]);
            Send(FaceKey.Cheek_Suck, lipWeight[LipShape_v2.Cheek_Suck]);
            var cheekPuff = (lipWeight[LipShape_v2.Cheek_Puff_Left] + lipWeight[LipShape_v2.Cheek_Puff_Right]) / 2.0f;
            Send(FaceKey.Cheek_Puff, cheekPuff);
            SendBipolar(FaceKey.Cheek_Suck_Puff, lipWeight[LipShape_v2.Cheek_Suck], cheekPuff);

            SendAverage(FaceKey.Mouth_Upper_Up, lipWeight[LipShape_v2.Mouth_Upper_UpLeft], lipWeight[LipShape_v2.Mouth_Upper_UpRight]);

            SendAverage(FaceKey.Mouth_Lower_Down, lipWeight[LipShape_v2.Mouth_Lower_DownLeft], lipWeight[LipShape_v2.Mouth_Lower_DownRight]);

            SendBipolar(FaceKey.Tongue_Left_Right, lipWeight[LipShape_v2.Tongue_Left], lipWeight[LipShape_v2.Tongue_Right]);
            SendBipolar(FaceKey.Tongue_Down_Up, lipWeight[LipShape_v2.Tongue_Down], lipWeight[LipShape_v2.Tongue_Up]);
        }

        void Send(string stringKey, FaceKey key, float value, float center = 0)
        {
            if (facePrefs.ContainsKey(key))
            {
                var pref = facePrefs[key];
                value = (value - center) * pref.gain + center;
                if (pref.isClipping) Mathf.Clamp(value, 0.0f, 1.0f);
                uiRows.SetValue(key, value);
                if (!pref.isSending) return;
            }

            var message = new OscMessage();
            message.address = addressRoot + stringKey;
            message.values.Add(value);
            osc.Send(message);

        }

        void Send(FaceKey key, float value, float center = 0)
        {
            Send(key.ToString(), key, value, center);
        }

        void SendBipolar(FaceKey key, float lowerValue, float higherValue)
        {
            Send(
                key,
                higherValue > lowerValue ? higherValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f,
                0.5f
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
            var center = facePrefs.ContainsKey(unitedKey) ? facePrefs[unitedKey].centerValue : 0.5f; // to make the centre value is 0.5 or the configured value
            Send(
                unitedKey,
                higherValue > lowerValue ? higherValue / 2.0f + center : -lowerValue / 2.0f + center
            );
        }

        void SendAverageAndBipolar(FaceKey unitedKey, float lower1, float lower2, float higher1, float higher2)
        {
            var higherValue = (higher1 + higher2) / 2.0f;
            var lowerValue = (lower1 + lower2) / 2.0f;

            var center = facePrefs.ContainsKey(unitedKey) ? facePrefs[unitedKey].centerValue : 0.5f; // to make the centre value is 0.5 or the configured value
            Send(
                unitedKey,
                higherValue > lowerValue ? higherValue / 2.0f + center : -lowerValue / 2.0f + center
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
            if (rotation > maxAngleRadian) rotation = maxAngleRadian;
            else if (rotation < -maxAngleRadian) rotation = -maxAngleRadian;
            rotation += facePrefs.ContainsKey(key) ? facePrefs[key].centerValue : 0.5f; // to make the centre value is 0.5 or the configured value

            Send(key, rotation / maxAngleRadian);
        }
        void SendRotationWithAverage(FaceKey key1, FaceKey key2, FaceKey keyAvarage, float rotation1, float rotation2)
        {
            var rotation = (rotation1 + rotation2) / 2.0f;
            SendRotation(key1, rotation1);
            SendRotation(key2, rotation2);
            SendRotation(keyAvarage, rotation);
        }

        public void UpdatePreference(FaceKey key, FaceValueRow value)
        {
            var pref = facePrefs[key];
            if (pref == null)
            {
                pref = new FaceDataPreferences(key);
                facePrefs.Add(key, pref);
            }
            pref.isSending = value.isSending;
            pref.gain = value.GetGain();
            pref.isClipping = value.isClipping;
            pref.center = value.center;
            isDirty = true;
        }

        public void Save()
        {
            var preferences = new Preferences();
            preferences.version = VERSION;
            preferences.faceDataPreferences = facePrefs.Values.ToArray();
            preferences.useEyeTracking = useEyeTracking.toggle.isOn;
            preferences.useFacialTracking = useEyeTracking.toggle.isOn;
            preferences.eyeTrackingType = eyeTrackingTypeList.options[eyeTrackingTypeList.value].text;
            preferences.isDebug = debugToggle.isOn;
            preferences.maxAngle = int.Parse(maxAngle.text);
            preferences.language = language.language;
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

                // Updater
                if (preferences.version < 1)
                {
                    preferences.faceDataPreferences = preferences.faceDataPreferences.Select(p => {
                        p.centerValue = 0.5f;
                        return p;
                    }).ToArray();
                }

                facePrefs = preferences.faceDataPreferences.ToDictionary(e => e.faceKey, e => e);
                foreach(var e in facePrefs)
                {
                    uiRows.InitRow(e.Key, e.Value.isSending, e.Value.gain, e.Value.isClipping, e.Value.center);
                }
                // Add absent keys with the default values
                foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
                {
                    if (!facePrefs.ContainsKey(key))
                    {
                        var val = new FaceDataPreferences(key);
                        facePrefs.Add(key, val);
                        uiRows.InitRow(key, val.isSending, val.gain, val.isClipping, val.center);
                    }
                }
                debugToggle.isOn = preferences.isDebug;
                // Disabled because Lip Tracker won't gently start
                // useEyeTracking.toggle.isOn = preferences.useEyeTracking;
                // useFacialTracking.toggle.isOn = preferences.useFacialTracking;
                maxAngle.text = preferences.maxAngle.ToString();

                try // parsing the value as a enum, and not change if the value does not exist
                {
                    var trackingTypeIndex = (int)Enum.Parse(typeof(EyeTrackingType), preferences.eyeTrackingType);
                    if (Enum.IsDefined(typeof(EyeTrackingType), trackingTypeIndex)) eyeTrackingTypeList.value = trackingTypeIndex;
                } catch { }
                OnEyeTrackerChanged();

                //if (!preferences.isDebug && preferences.useFacialTracking) SwitchFacialTracker(true);

                language.language = preferences.language;
                uiRows.RefreshView();

                if (preferences.version < VERSION)
                {
                    Save();
                }
                return facePrefs;
            }
            catch (Exception)
            {
                return Enum.GetValues(typeof(FaceKey)).Cast<FaceKey>().ToDictionary(key => key, key => {
                    var p = new FaceDataPreferences(key);
                    if (FaceKeyUtils.GetDataType(key) == DataType.Gaze) p.centerValue = 0;
                    return p;
                });
            }
        }

        public void MakeDirty()
        {
            isDirty = true;
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
        public int version;
        public FaceDataPreferences[] faceDataPreferences;
        public bool isDebug = false;
        public bool useEyeTracking = true;
        public bool useFacialTracking = true;
        public string eyeTrackingType;
        public float maxAngle = 45f;
        public string language = "";
    }

    [Serializable]
    public class FaceDataPreferences
    {
        public string key;
        public bool isSending = true;
        public float gain = 1;
        public bool isClipping = true;
        public float centerValue = 0.5f;

        public FaceKey faceKey { get { return (FaceKey)Enum.Parse(typeof(FaceKey), key); } }
        public Center center
        {
            get
            {
                var def = FaceKeyUtils.DefaultValue(faceKey);
                if (0.5f - 0.0009765625f < def && def < 0.5f + 0.0009765625)
                {
                    return centerValue < float.Epsilon ? Center.Zero : Center.Half;
                }
                else
                {
                    return Center.Fixed;
                }
            }
            set
            {
                switch (value) {
                    case Center.Fixed:
                        centerValue = 0.5f;
                        return;
                    case Center.Zero:
                        centerValue = 0;
                        return;
                    case Center.Half:
                        centerValue = 0.5f;
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public FaceDataPreferences(string key)
        {
            this.key = key;
        }
        public FaceDataPreferences(FaceKey key)
        {
            this.key = key.ToString();
        }
        public FaceDataPreferences(string key, bool isSending, float gain, bool isClipping, float centerValue)
        {
            this.key = key;
            this.isSending = isSending;
            this.gain = gain;
            this.isClipping = isClipping;
            this.centerValue = centerValue;
        }
    }

    public enum DeviceState
    {
        Stopping, Disbled, Starting, Enabled, Unavailable
    }
    public enum EyeTrackingType
    {
        ViveSRanipal,
        PimaxAsee,
    }
}
