using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Range(0f, 100f)] public float MasterVolume = 100f;
    [Range(0f, 100f)] public float MusicVolume = 100f;
    [Range(0f, 100f)] public float SFXVolume = 100f;

    [Header("Accessibility Settings")]
    public bool TunnelingVignette = true;
    [Range(0f, 1f)] public float TunnelingVignetteAperatureSize = .757f;
    [Range(0f, 1f)] public float TunnelingVignetteFeathering = .291f;

    public bool SmoothTurning = false;
    [Range(60f, 300f)] public float SmoothTurningSpeed = 60f;


    public bool ToggleGrab = false;
    public bool RangedInteractors = false;

    public AudioMixer audioMixer;
    AudioMixerGroup MasterAudio;
    AudioMixerGroup Music;
    AudioMixerGroup SFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.parent);
        }
        else
        {
            Destroy(gameObject);
        }

        AudioMixer audioMixer = Resources.Load<AudioMixer>("Audio/MainAudioMixer");
        MasterAudio = audioMixer.FindMatchingGroups("Master")[0]; // Find the Master group in the audio mixer
        Music = audioMixer.FindMatchingGroups("Music")[0]; // Find the Music group in the audio mixer
        SFX = audioMixer.FindMatchingGroups("SFX")[0]; // Find the SFX group in the audio mixer

        SetMusicVolume(MusicVolume);
        SetSFXVolume(SFXVolume);
        SetMasterVolume(MasterVolume);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
        float decibelVolume = VolumePercentToDecibel(MasterVolume);
        MasterAudio.audioMixer.SetFloat("MasterVolume", decibelVolume);
        Debug.Log($"Master Volume set to {decibelVolume} dB ({MasterVolume}%)");
    }
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
        float decibelVolume = VolumePercentToDecibel(MusicVolume);
        Music.audioMixer.SetFloat("MusicVolume", decibelVolume);
        Debug.Log($"Music Volume set to {decibelVolume} dB ({MusicVolume}%)");
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        float decibelVolume = VolumePercentToDecibel(SFXVolume);
        SFX.audioMixer.SetFloat("SFXVolume", decibelVolume);
        Debug.Log($"SFX Volume set to {decibelVolume} dB ({SFXVolume}%)");
    }

    float VolumePercentToDecibel(float volumePercent)
    {
        if (volumePercent <= 0f)
            return -80f;
        Debug.Log($"Converting volume percent {volumePercent} to decibel.");
        return Mathf.Log10(volumePercent/100) * 20f;
    }

    public void DisableTutorialSettings()
    {
        RangedInteractors = false;
    }
}
