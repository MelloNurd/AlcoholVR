using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static int audioSourceCount = 5;

    private static List<AudioSource> audioSources = new List<AudioSource>();
    private static int currentAudioSourceIndex = 0;

    private void Awake()
    {
        for (int i = 0; i < audioSourceCount; i++)
        {
            GameObject audioSourceObject = new GameObject($"ItemSoundAudioSource_{i}");
            audioSourceObject.transform.parent = transform;
            AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
            
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 8f;

            audioSources.Add(audioSource);
        }
    }

    private static AudioSource GetNextSource()
    {
        AudioSource nextAudioSource = audioSources[currentAudioSourceIndex];
        currentAudioSourceIndex = (currentAudioSourceIndex + 1) % audioSourceCount;
        return nextAudioSource;
    }

    public static AudioSource PlaySoundAtPoint(AudioClip audioClip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (audioSources.Count == 0 || audioClip == null)
        {
            Debug.LogError("Unable to play sound: No audio sources available or audio clip is null.");
            return null;
        }

        AudioSource audioSource = GetNextSource();

        audioSource.transform.position = position;

        audioSource.pitch = pitch;
        audioSource.volume = volume;

        audioSource.PlayOneShot(audioClip);

        return audioSource;
    }
}
