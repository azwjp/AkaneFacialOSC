using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace AZW.FaceOSC
{
    [RequireComponent(typeof(FacialCapture))]
    public class ValueRowsUI : MonoBehaviour
    {
        [SerializeField] FacialCapture manager;
        [Header("UI Components")]
        [SerializeField] GameObject rowPrefab;
        [SerializeField] Transform parentView;
        [SerializeField] Toggle allCheck;

        public Dictionary<FaceKey, FaceValueRow> valueRows = new Dictionary<FaceKey, FaceValueRow>();

        void Start()
        {
            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                var prefab = Instantiate(rowPrefab, parentView);
                var faceVal = prefab.GetComponent<FaceValueRow>();

                faceVal.manager = manager;
                faceVal.faceKey = key;
                faceVal.rows = this;

                valueRows.Add(key, faceVal);
            }
        }

        public void RefreshView()
        {
            OnChildCheckboxStateChanged();
        }

        public void ShowAll()
        {
            valueRows.Values.ToList().ForEach(valueRow => valueRow.gameObject.SetActive(true));
            RefreshView();
        }
        public void ShowEssentials ()
        {
            valueRows.Values.ToList().ForEach(valueRow => {
                switch (valueRow.faceKey)
                {
                    case FaceKey.Eye_Left_Blink:
                    case FaceKey.Eye_Right_Blink:
                    case FaceKey.Gaze_Horizontal:
                    case FaceKey.Gaze_Vertical:
                    case FaceKey.Jaw_Open:
                    case FaceKey.Mouth_Pout:
                    case FaceKey.Mouth_Smile:
                    case FaceKey.Mouth_Sad:
                    case FaceKey.Mouth_Sad_Smile:
                    case FaceKey.Cheek_Puff:
                    case FaceKey.Cheek_Suck:
                    case FaceKey.Tongue_LongStep1:
                        valueRow.gameObject.SetActive(true);
                        return;
                    default:
                        valueRow.gameObject.SetActive(false);
                        return;
                }
            });
            RefreshView();
        }
        public void ShowEye()
        {
            valueRows.Values.ToList().ForEach(valueRow => valueRow.gameObject.SetActive(FaceKeyUtils.GetDataType(valueRow.faceKey) == DataType.Eye));
            RefreshView();
        }

        public void ShowComputedEye()
        {
            valueRows.Values.ToList().ForEach(valueRow => valueRow.gameObject.SetActive(FaceKeyUtils.GetDataType(valueRow.faceKey) == DataType.ComputedEye || FaceKeyUtils.GetDataType(valueRow.faceKey) == DataType.Gaze));
            RefreshView();
        }
        public void ShowFacial()
        {
            valueRows.Values.ToList().ForEach(valueRow => valueRow.gameObject.SetActive(FaceKeyUtils.GetDataType(valueRow.faceKey) == DataType.Facial));
            RefreshView();
        }
        public void ShowComputedFacial()
        {
            valueRows.Values.ToList().ForEach(valueRow => valueRow.gameObject.SetActive(FaceKeyUtils.GetDataType(valueRow.faceKey) == DataType.ComputedFacial));
            RefreshView();
        }
        public void OnAllCheckChanged(bool newValue)
        {
            valueRows.Values.Where(valueRow => valueRow.gameObject.activeInHierarchy).ToList().ForEach(valueRow => valueRow.isSending = newValue);
            RefreshView();
        }
        public void OnChildCheckboxStateChanged()
        {
            bool isAllTrue = valueRows.Values.Where(valueRow => valueRow.gameObject.activeInHierarchy).All(valueRow => valueRow.isSending);
            allCheck.SetIsOnWithoutNotify(isAllTrue);
        }

        public void SetGain(FaceKey key, float val)
        {
            FaceValueRow valueRow;
            if (valueRows.TryGetValue(key, out valueRow))
            {
                valueRow.SetGain(val);
            }
        }
        public void SetValue(FaceKey key, float val)
        {
            FaceValueRow valueRow;
            if (valueRows.TryGetValue(key, out valueRow))
            {
                valueRow.SetValue(val);
            }
        }

        public void InitRow(FaceKey key, bool isOn, float gain)
        {
            var row = valueRows[key];
            row.isSending = isOn;
            row.SetGain(gain);
        }
    }
}