using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class Autosim : MonoBehaviour
{
    [SerializeField] GameObject InputSimulator;

    private bool lastDeviceState = false;
    private XRInputSubsystem xrInput;

    void Start()
    {
        // Get the XR Input Subsystem  
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subsystems); // Updated to use GetSubsystems instead of GetInstances  

        if (subsystems.Count > 0)
        {
            xrInput = subsystems[0];
        }

        if (xrInput != null)
        {
            lastDeviceState = xrInput.running;
            InputSimulator.SetActive(!lastDeviceState);
        }
        else
        {
            Debug.LogWarning("No XRInputSubsystem found. Assuming headset not active.");
            InputSimulator.SetActive(true);
        }
    }

    void Update()
    {
        if (xrInput == null)
            return;

        bool isRunning = xrInput.running;

        if (isRunning != lastDeviceState)
        {
            lastDeviceState = isRunning;

            if (isRunning)
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
