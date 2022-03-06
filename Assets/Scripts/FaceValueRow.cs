using UnityEngine;
using UnityEngine.UI;

namespace AZW.FaceOSC
{
    public class FaceValueRow : MonoBehaviour
    {
        public FacialCapture manager;
        public ValueRowsUI rows;
        public FaceKey faceKey;
        [SerializeField] Text address;
        [SerializeField] Toggle isSendingToggle;
        [SerializeField] Text valueLabel;
        [SerializeField] Slider gainSlider;
        [SerializeField] InputField gainValue;
        [SerializeField] Toggle clip;

        public bool isSending
        {
            get { return isSendingToggle.isOn; }
            set { isSendingToggle.isOn = value; }
        }

        public bool isClipping
        {
            get { return clip.isOn; }
            set { clip.isOn = value; }
        }

        void Start()
        {
            if (manager == null) manager = GetComponentInParent<FacialCapture>();
            address.text = faceKey.ToString();
            isSendingToggle.onValueChanged.AddListener(newVal => {
                manager.UpdatePreference(faceKey, this);
            });
            gainSlider.onValueChanged.AddListener(newVal =>
            {
                gainValue.SetTextWithoutNotify(newVal.ToString());
                manager.UpdatePreference(faceKey, this);
            });
            gainValue.onValueChanged.AddListener(newVal =>
            {
                gainSlider.SetValueWithoutNotify(float.Parse(newVal));
                manager.UpdatePreference(faceKey, this);
            });
            clip.onValueChanged.AddListener(newVal =>
            {
                manager.UpdatePreference(faceKey, this);
            });
        }

        public void OnChangedIsSending()
        {
            rows.OnChildCheckboxStateChanged();
        }

        public float GetGain() { return gainSlider.value; }
        public void SetGain(float value)
        {
            gainSlider.SetValueWithoutNotify(value);
            gainValue.SetTextWithoutNotify(value.ToString());
        }

        public void SetValue(float value) { valueLabel.text = value.ToString("0.00"); }
    }
}