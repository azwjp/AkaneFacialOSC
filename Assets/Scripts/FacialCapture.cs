using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Lip;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;

namespace AZW.FaceOSC
{
    public class FacialCapture : MonoBehaviour
    {
        [Header("OSC")]
        public OSC osc;
        Dictionary<LipShape_v2, float> lipWeight = new Dictionary<LipShape_v2, float>();
        [SerializeField]
        string addressRoot = "/avatar/parameters/";

        [Header("UI Components")]
        [SerializeField] GameObject rowPrefab;
        [SerializeField] Transform parentView;
        [SerializeField] SaveButton saveButton;
        [SerializeField] Toggle debugToggle;


        Dictionary<FaceKey, FaceDataPreferences> prefs = new Dictionary<FaceKey, FaceDataPreferences>();
        Dictionary<FaceKey, FaceValueRow> valueRows = new Dictionary<FaceKey, FaceValueRow>();
        bool isDebug = false;
        const string PREF_FILE_PATH = "./preferences.json";


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
                var prefab = Instantiate(rowPrefab, parentView);
                var faceVal = prefab.GetComponent<FaceValueRow>();

                faceVal.manager = this;
                faceVal.faceKey = key;

                valueRows.Add(key, faceVal);
                var val = new FaceDataPreferences();
                prefs.Add(key, val);

                faceVal.SetGain(val.gain);
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
                FacialData();
                ComputedData();
            }
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

        bool FacialData()
        {
            foreach (var param in lipWeight)
            {
                var stringKey = param.Key.ToString();
                var key = (FaceKey)Enum.Parse(typeof(FaceKey), stringKey);
                var value = param.Value;

                Send(stringKey, key, value);
            }

            return true;
        }
        void ComputedData()
        {
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
                valueRows[key].SetValue(value);
                if (!pref.isSending) return;
            }

            var message = new OscMessage();
            message.address = addressRoot + stringKey;
            message.values.Add(value);
            osc.Send(message);

        }

        void SendBipolar(FaceKey key, float upperValue, float lowerValue)
        {
            Send(
                key.ToString(),
                key,
                upperValue > lowerValue ? upperValue / 2.0f + 0.5f : -lowerValue / 2.0f + 0.5f
            );
        }
        void SendAverage(FaceKey key, float value1, float value2)
        {
            Send(
                key.ToString(),
                key,
                (value1 + value2) / 2.0f
            );
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
                return JsonUtility.FromJson<Dictionary<FaceKey, FaceDataPreferences>>(json);
            }
            catch (Exception e)
            {
                return valueRows.Keys.ToDictionary(key => key, key => new FaceDataPreferences());
            }
        }

        public void ToggleDebug()
        {
            isDebug = debugToggle.isOn;
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
