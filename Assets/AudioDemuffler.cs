using UnityEngine;

public class AudioDemuffler : MonoBehaviour
{
    public AudioLowPassFilter[] audioLowPassFilters;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            // Enable all AudioLowPassFilters
            foreach (var filter in audioLowPassFilters)
            {
                filter.enabled = false;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            // Disable all AudioLowPassFilters
            foreach (var filter in audioLowPassFilters)
            {
                filter.enabled = true;
            }
        }
    }
}
