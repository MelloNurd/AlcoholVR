using UnityEngine;
using UnityEngine.UI;

public class PhoneSettings : MonoBehaviour
{
    [SerializeField] Slider MasterVolumeSlider;
    [SerializeField] Slider MusicVolumeSlider;
    [SerializeField] Slider SFXVolumeSlider;
    [SerializeField] Toggle TunnelingVignetteToggle;
    [SerializeField] Slider AperatureSizeSlider;
    [SerializeField] Slider AperatureFeatheringSlider;
    [SerializeField] Toggle SmoothTurningToggle;
    [SerializeField] Slider SmoothTurningSpeedSlider;
    [SerializeField] Toggle ToggleGrabToggle;
    [SerializeField] Toggle RangedInteractorsToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MasterVolumeSlider.value = SettingsManager.Instance.MasterVolume;
        MusicVolumeSlider.value = SettingsManager.Instance.MusicVolume;
        SFXVolumeSlider.value = SettingsManager.Instance.SFXVolume;
        TunnelingVignetteToggle.isOn = SettingsManager.Instance.TunnelingVignette;
        AperatureSizeSlider.value = SettingsManager.Instance.TunnelingVignetteAperatureSize;
        AperatureFeatheringSlider.value = SettingsManager.Instance.TunnelingVignetteFeathering;
        SmoothTurningToggle.isOn = SettingsManager.Instance.SmoothTurning;
        SmoothTurningSpeedSlider.value = SettingsManager.Instance.SmoothTurningSpeed;
        ToggleGrabToggle.isOn = SettingsManager.Instance.ToggleGrab;
        RangedInteractorsToggle.isOn = SettingsManager.Instance.RangedInteractors;
    }

    public void OnMasterVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMasterVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SettingsManager.Instance.SetSFXVolume(value);
    }

    public void OnTunnelingVignetteToggleChanged()
    {
        Player.Instance.ToggleTunnelingVignette();
    }

    public void OnAperatureSizeChanged(float value)
    {
        Player.Instance.SetTunnelingVignetteAperatureSize(value);
    }

    public void OnAperatureFeatheringChanged(float value)
    {
        Player.Instance.SetTunnelingVignetteFeathering(value);
    }

    public void OnSmoothTurningToggleChanged(bool value)
    {
        Player.Instance.ToggleSmoothTurning(value);
    }

    public void OnSmoothTurningSpeedChanged(float value)
    {
        Player.Instance.SetSmoothTurningSpeed(value);
    }

    public void OnToggleGrabChanged(bool value)
    {
        Player.Instance.ToggleGrabToggle(value);
    }

    public void OnRangedInteractorsChanged(bool value)
    {
        SettingsManager.Instance.RangedInteractors = value;
        Player.Instance.ToggleRangedInteractors(value);
    }
}
