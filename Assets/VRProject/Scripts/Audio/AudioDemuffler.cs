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

    public void AddLowPassFilter(AudioLowPassFilter filter)
    {
        // Create a new array with one more element
        AudioLowPassFilter[] newArray = new AudioLowPassFilter[audioLowPassFilters.Length + 1];
        // Copy the old array to the new array
        for (int i = 0; i < audioLowPassFilters.Length; i++)
        {
            newArray[i] = audioLowPassFilters[i];
        }
        // Add the new filter to the end of the new array
        newArray[newArray.Length - 1] = filter;
        // Replace the old array with the new array
        audioLowPassFilters = newArray;
    }
}
