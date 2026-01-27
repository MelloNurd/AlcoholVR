using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager Instance { get; set; }

    public static int audioSourceCount = 5;

    private static List<AudioSource> audioSources = new List<AudioSource>();
    private static int currentAudioSourceIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

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
        if (audioSources.Count == 0)
        {
            Debug.LogError("Unable to play sound: No audio sources available");
            return null;
        }
        if (audioClip == null)
        {
            Debug.LogError("Unable to play sound: Audio clip is null.");
            return null;
        }

        AudioSource audioSource = GetNextSource();

        audioSource.transform.position = position;

        audioSource.pitch = pitch;
        audioSource.volume = volume;

        audioSource.PlayOneShot(audioClip);

        return audioSource;
    }

    public static async UniTask<AudioSource> PlaySoundAttached(AudioClip audioClip, Transform parentTransform, float volume = 1f, float pitch = 1f)
    {
        if (audioSources.Count == 0 || audioClip == null)
        {
            Debug.LogError("Unable to play sound: No audio sources available or audio clip is null.");
            return null;
        }

        AudioSource audioSource = GetNextSource();

        audioSource.transform.SetParent(parentTransform);
        audioSource.transform.localPosition = Vector3.zero;

        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.PlayOneShot(audioClip);

        await UniTask.Delay(audioClip.length.ToMS());

        audioSource.transform.SetParent(Instance.transform);

        return audioSource;
    }
}
