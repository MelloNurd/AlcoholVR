using UnityEngine;
using UnityEngine.XR;

public class LiveHeadsetChecker : MonoBehaviour
{
    [SerializeField] GameObject InputSimulator;

    private bool lastDeviceState = false;

    private void Start()
    {
        // Initialize the last device state
        lastDeviceState = XRSettings.isDeviceActive;
        // Set the initial state of InputSimulator based on the current device state
        InputSimulator.SetActive(!lastDeviceState);
    }

    void Update()
    {
        bool isActive = XRSettings.isDeviceActive;

        // Only update if the state changes
        if (isActive != lastDeviceState)
        {
            lastDeviceState = isActive;

            if (isActive)
            {
                Debug.Log("VR headset connected.");
                InputSimulator.SetActive(false);
            }
            else
            {
                Debug.Log("VR headset not detected.");
                InputSimulator.SetActive(true);
            }
        }
    }
}
