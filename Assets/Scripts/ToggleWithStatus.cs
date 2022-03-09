using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AZW.FaceOSC
{
    public class ToggleWithStatus : MonoBehaviour
    {
        public Toggle toggle;
        [SerializeField] Text stopped;
        [SerializeField] Text starting;
        [SerializeField] Text stopping;
        [SerializeField] Text running;
        [SerializeField] Text unavailable;

        public void SetState(DeviceState state)
        {
            switch (state)
            {
                case DeviceState.Disbled:
                    stopped.gameObject.SetActive(true);
                    starting.gameObject.SetActive(false);
                    stopping.gameObject.SetActive(false);
                    running.gameObject.SetActive(false);
                    unavailable.gameObject.SetActive(false);
                    break;
                case DeviceState.Starting:
                    stopped.gameObject.SetActive(false);
                    starting.gameObject.SetActive(true);
                    stopping.gameObject.SetActive(false);
                    running.gameObject.SetActive(false);
                    unavailable.gameObject.SetActive(false);
                    break;
                case DeviceState.Stopping:
                    stopped.gameObject.SetActive(false);
                    starting.gameObject.SetActive(false);
                    stopping.gameObject.SetActive(true);
                    running.gameObject.SetActive(false);
                    unavailable.gameObject.SetActive(false);
                    break;
                case DeviceState.Enabled:
                    stopped.gameObject.SetActive(false);
                    starting.gameObject.SetActive(false);
                    stopping.gameObject.SetActive(false);
                    running.gameObject.SetActive(true);
                    unavailable.gameObject.SetActive(false);
                    break;
                case DeviceState.Unavailable:
                    stopped.gameObject.SetActive(false);
                    starting.gameObject.SetActive(false);
                    stopping.gameObject.SetActive(false);
                    running.gameObject.SetActive(false);
                    unavailable.gameObject.SetActive(true);
                    break;
            }
        }
    }
}