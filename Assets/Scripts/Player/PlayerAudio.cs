using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance { get; private set; }

    private int audioSourceCount = 8;
    private List<AudioSource> audioSources = new();
    private int currentAudioSourceIndex = 0;

    AudioMixerGroup SFXGroup;

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

        AudioMixer audioMixer = Resources.Load<AudioMixer>("Audio/MainAudioMixer");
        SFXGroup = audioMixer.FindMatchingGroups("SFX")[0]; // Find the SFX group in the audio mixer

        // Initialize audio sources
        audioSources = new List<AudioSource>();
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
        source.outputAudioMixerGroup = SFXGroup; // Set the audio mixer group for the source
        audioSources.Add(source); // Fix: Add AudioSource to the List 
    }

    /// <summary>
    /// Play a one-shot sound effect.
    /// </summary>
    /// <param name="audio">Audio to play</param>
    /// <param name="volume">(0, 1f) volume to play the audio at</param>
    /// <param name="randomizePitch">Whether or not to randomize the pitch of the audio (from 0.9f to 1.1f).</param>
    /// <returns>The audio source playing the sound.</returns>
    public static void PlaySound(AudioClip audio, float volume, out AudioSource source,  bool randomizePitch = false)
    {
        source = null;

        if (Instance == null) return;
        if(audio == null)
        {
            Debug.LogWarning($"Could not find audio {audio.name}. Unable to play.");
            return;
        }

        source = Instance.audioSources[Instance.currentAudioSourceIndex];
        source.clip = audio;
        source.pitch = randomizePitch ? Random.Range(0.9f, 1.1f) : 1f;
        source.volume = volume;
        source.Play();

        Instance.currentAudioSourceIndex = (Instance.currentAudioSourceIndex + 1) % Instance.audioSourceCount;
    }
    public static void PlaySound(AudioClip audio) => PlaySound(audio, 1f, out _, false);

    /// <summary>
    /// Play a sound that will loop forever. This will remove it from the pool of available audio sources.
    /// </summary>
    /// <returns>The audio source dedicated to playing this sound.</returns>
    public static AudioSource PlayLoopingSound(AudioClip audio, float volume)
    {
        if (Instance == null || audio == null) return null;
        if (Instance.audioSources.Count <= 0) return null;

        AudioSource source = Instance.audioSources[Instance.audioSources.Count - 1];
        source.clip = audio;
        source.loop = true;
        source.volume = volume;
        source.Play();

        Instance.audioSources.Remove(source); // Remove this source from list as it is constant
        Instance.AddNewAudioSource(); // Replace it with a new audio source

        return source;
    }
    public static AudioSource PlayLoopingSound(AudioClip audio) => PlayLoopingSound(audio, 1f);

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
    public static void StopAllSounds() => StopAllSounds(false);
}
