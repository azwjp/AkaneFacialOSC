using UnityEngine;
using UnityEngine.UI;

namespace AZW.FaceOSC
{
    [RequireComponent(typeof(Button))]
    public class SaveButton : MonoBehaviour
    {
        string originalText;
        Button button;
        Text label;

        void Start()
        {
            button = GetComponent<Button>();
            label = GetComponentInChildren<Text>();
            originalText = label.text;
        }
        public void SetDirty(bool isDirty)
        {
            var button = GetComponent<Button>();
            if (isDirty)
            {
                label.text = originalText + "*";
                button.interactable = true;
            }
            else
            {
                label.text = originalText;
                button.interactable = false;
            }
        }
    }
}