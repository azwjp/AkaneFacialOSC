using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZW.FaceOSC
{
    [RequireComponent(typeof(FacialCapture))]
    public class ValueRowsUI : MonoBehaviour
    {
        [SerializeField] FacialCapture manager;
        [Header("UI Components")]
        [SerializeField] GameObject rowPrefab;
        [SerializeField] Transform parentView;

        public Dictionary<FaceKey, FaceValueRow> valueRows = new Dictionary<FaceKey, FaceValueRow>();

        void Start()
        {
            foreach (FaceKey key in Enum.GetValues(typeof(FaceKey)))
            {
                var prefab = Instantiate(rowPrefab, parentView);
                var faceVal = prefab.GetComponent<FaceValueRow>();

                faceVal.manager = manager;
                faceVal.faceKey = key;

                valueRows.Add(key, faceVal);
            }
        }

        void Update()
        {

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
    }
}