using UnityEngine;

public class AudioDemuffler : MonoBehaviour
{
    public AudioLowPassFilter[] audioLowPassFilters;
    AudioLowPassFilter audioLowPassFilter;

    private void Start()
    {
        audioLowPassFilter = ContinuousMusic.Instance.GetComponent<AudioLowPassFilter>();
        //add the AudioLowPassFilter to the array
        audioLowPassFilters = new AudioLowPassFilter[] { audioLowPassFilter };
    }

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
