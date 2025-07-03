using UnityEngine;
using UnityEngine.XR;

public class HeadsetChecker : MonoBehaviour
{
    [SerializeField] GameObject InputSimulator;

    void Start()
    {
        if (XRSettings.isDeviceActive)
        {
            InputSimulator.SetActive(false);
        }
        else
        {
            InputSimulator.SetActive(true);
        }
    }
}
