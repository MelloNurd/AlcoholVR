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
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMusicVolume(VolumePercentToDecibel(MusicVolume));
        SetSFXVolume(VolumePercentToDecibel(SFXVolume));
        SetMasterVolume(VolumePercentToDecibel(MasterVolume));
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
    }
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
        float decibelVolume = VolumePercentToDecibel(MusicVolume);
        Music.audioMixer.SetFloat("MusicVolume", decibelVolume);
    }

    public void SetSFXVolume(float volume)
    {
        SFXVolume = volume;
        float decibelVolume = VolumePercentToDecibel(SFXVolume);
        SFX.audioMixer.SetFloat("SFXVolume", decibelVolume);
    }

    float VolumePercentToDecibel(float volumePercent)
    {
        float volume01 = Mathf.Clamp01(volumePercent / 100f);
        return Mathf.Log10(volume01) * 20f;
    }

    public void DisableTutorialSettings()
    {
        RangedInteractors = false;
    }
}
