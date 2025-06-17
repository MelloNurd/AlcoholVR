using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance { get; private set; }

    private int audioSourceCount = 8;
    private List<AudioSource> audioSources = new();
    private int currentAudioSourceIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioSources = new List<AudioSource>(); // Fix: Initialize as a List instead of an array  
        for (int i = 0; i < audioSourceCount; i++)
        {
            AddNewAudioSource();
        }
    }

    private void AddNewAudioSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        audioSources.Add(source); // Fix: Add AudioSource to the List 
    }

    /// <summary>
    /// Play a one-shot sound effect.
    /// </summary>
    /// <param name="audio">Audio to play</param>
    /// <param name="randomizePitch">Whether or not to randomize the pitch of the audio (from 0.9f to 1.1f).</param>
    public static void PlaySound(AudioClip audio, bool randomizePitch = false)
    {
        if (Instance == null || audio == null) return;

        AudioSource source = Instance.audioSources[Instance.currentAudioSourceIndex];
        source.clip = audio;
        source.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
        source.Play();

        Instance.currentAudioSourceIndex = (Instance.currentAudioSourceIndex + 1) % Instance.audioSourceCount;
    }

    /// <summary>
    /// Play a sound that will loop forever. This will remove it from the pool of available audio sources.
    /// </summary>
    /// <returns>The audio source dedicated to playing this sound.</returns>
    public static AudioSource PlayLoopingSound(AudioClip audio)
    {
        if (Instance == null || audio == null) return null;

        AudioSource source = Instance.audioSources[Instance.audioSources.Count - 1];
        source.clip = audio;
        source.loop = true;
        source.Play();

        Instance.audioSources.Remove(source); // Remove this source from list as it is constant
        Instance.AddNewAudioSource(); // Replace it with a new audio source

        return source;
    }

    /// <summary>
    /// Stops all currently playing sounds.
    /// </summary>
    /// <param name="stopLoopingSounds">Enable to also stop looping sounds.</param>
    public static void StopAllSounds(bool stopLoopingSounds = false)
    {
        if (Instance == null) return;

        if (stopLoopingSounds)
        {
            foreach(var source in Instance.gameObject.GetComponents<AudioSource>())
            {
                source.Stop();
            }
        }
        else
        {
            foreach (var source in Instance.audioSources)
            {
                source.Stop();
            }
        }
    }
}
