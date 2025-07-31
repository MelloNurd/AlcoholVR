using System.Collections.Generic;
using UnityEngine;

public class RandomSounds : MonoBehaviour
{
    // take a list of Audio Clips and play a random one at random intervals
    public List<AudioClip> audioClips = new List<AudioClip>();
    public float minInterval = 1f;
    public float maxInterval = 8f;
    private AudioSource audioSource;
    private AudioClip audioClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioClips.Count > 0)
        {
            PlayRandomSound();
        }
        else
        {
            Debug.LogWarning("No audio clips assigned to RandomSounds.");
        }
    }

    void PlayRandomSound()
    {
        if (audioClips.Count == 0)
        {
            Debug.LogWarning("No audio clips available to play.");
            return;
        }
        audioClip = audioClips[Random.Range(0, audioClips.Count)];
        audioSource.clip = audioClip;
        audioSource.Play();
        // Schedule the next sound
        float nextInterval = Random.Range(minInterval, maxInterval);
        Invoke(nameof(PlayRandomSound), nextInterval);
    }
}
