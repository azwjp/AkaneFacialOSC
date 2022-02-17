using UnityEngine;
using UnityEngine.UI;

namespace AZW.FaceOSC
{
    public class FaceValueRow : MonoBehaviour
    {
        public FacialCapture manager;
        public FaceKey faceKey;
        [SerializeField] Text address;
        [SerializeField] Toggle isSending;
        [SerializeField] Text valueLabel;
        [SerializeField] Slider gain;
        [SerializeField] InputField gainValue;

        void Start()
        {
            if (manager == null) manager = GetComponentInParent<FacialCapture>();
            address.text = faceKey.ToString();
            isSending.onValueChanged.AddListener(newVal => manager.UpdatePreference(faceKey, this));
            gain.onValueChanged.AddListener(newVal =>
            {
                gainValue.SetTextWithoutNotify(newVal.ToString());
                manager.UpdatePreference(faceKey, this);
            });
            gainValue.onValueChanged.AddListener(newVal =>
            {
                gain.SetValueWithoutNotify(float.Parse(newVal));
                manager.UpdatePreference(faceKey, this);
            });
        }

        public bool IsSending() { return isSending.isOn; }

        public float GetGain() { return gain.value; }
        public void SetGain(float value)
        {
            gain.SetValueWithoutNotify(value);
            gainValue.SetTextWithoutNotify(value.ToString());
        }

        public void SetValue(float value) { valueLabel.text = value.ToString("0.00"); }
    }
}