using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsManager
{
    public const string FirstPlayKey = "FirstPlay";
    public const string MaxHealthSliderKey = "MaxHealthSlider";
    public const string MaxHealthTextKey = "MaxHealthText";
    public const string FullScreenToggleKey = "FullscreenToggle";
    public const string ResolutionDropdownKey = "ResolutionDropdown";
    public const string AntialiasingDropdownKey = "AntialiasingDropdown";
    public const string VSyncDropdownKey = "VSyncDropdown";
    public const string FrameRateDropdownKey = "FrameRateDropdown";
    public const string MasterSliderKey = "MasterSlider";
    public const string MasterTextKey = "MasterText";
    public const string MusicSliderKey = "MusicSlider";
    public const string MusicTextKey = "MusicText";
    public const string FXSliderKey = "FXSlider";
    public const string FXTextKey = "FXText";

    // Gameplay settings
    public float HealthPercent { get; private set; } = 1f;
    // Graphics settings
    public bool Fullscreen { get; private set; } = true;
    public int ResolutionIndex { get; private set; } = 0;
    public int AntialiasingIndex { get; private set; } = 0;
    public int VSyncIndex { get; private set; } = 0;
    public int FrameRateIndex { get; private set; } = 0;
    // Sound settings
    public float MasterVolume { get; private set; } = 0f;
    public float MusicVolume { get; private set; } = 0f;
    public float FXVolume { get; private set; } = 0f;
    // Controls settings


    public void SetSettings(UIManager uiManager)
    {
        // Gameplay
        Slider maxHealthElement = uiManager.GetSettingsElement<Slider>(MaxHealthSliderKey);
        HealthPercent = maxHealthElement.value;

        // Graphics

        Toggle fullscreenElement = uiManager.GetSettingsElement<Toggle>(FullScreenToggleKey);
        Fullscreen = fullscreenElement.isOn;

        TMP_Dropdown resolutionElement = uiManager.GetSettingsElement<TMP_Dropdown>(ResolutionDropdownKey);
        ResolutionIndex = resolutionElement.value;

        TMP_Dropdown antialiasingElement = uiManager.GetSettingsElement<TMP_Dropdown>(AntialiasingDropdownKey);
        AntialiasingIndex = antialiasingElement.value;

        TMP_Dropdown vsyncElement = uiManager.GetSettingsElement<TMP_Dropdown>(VSyncDropdownKey);
        VSyncIndex = vsyncElement.value;

        TMP_Dropdown frameRateElement = uiManager.GetSettingsElement<TMP_Dropdown>(FrameRateDropdownKey);
        FrameRateIndex = frameRateElement.value;

        // Sound

        Slider masterVolumeElement = uiManager.GetSettingsElement<Slider>(MasterSliderKey);
        MasterVolume = masterVolumeElement.value;

        Slider musicVolumeElement = uiManager.GetSettingsElement<Slider>(MusicSliderKey);
        MusicVolume = musicVolumeElement.value;

        Slider fxVolumeElement = uiManager.GetSettingsElement<Slider>(FXSliderKey);
        FXVolume = fxVolumeElement.value;

        // Controls

    }
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(FirstPlayKey))
        {
            // Gameplay
            HealthPercent = PlayerPrefs.GetFloat(MaxHealthSliderKey);

            // Graphics
            Fullscreen = GetBool(FullScreenToggleKey);
            ResolutionIndex = PlayerPrefs.GetInt(ResolutionDropdownKey);
            AntialiasingIndex = PlayerPrefs.GetInt(AntialiasingDropdownKey);
            VSyncIndex = PlayerPrefs.GetInt(VSyncDropdownKey);
            FrameRateIndex = PlayerPrefs.GetInt(FrameRateDropdownKey);

            // Sound
            MasterVolume = PlayerPrefs.GetFloat(MasterSliderKey);
            MusicVolume = PlayerPrefs.GetFloat(MusicSliderKey);
            FXVolume = PlayerPrefs.GetFloat(FXSliderKey);

            // Controls
        }
    }
    public void SaveSettings()
    {
        SetBool(FirstPlayKey, false);

        // Gameplay
        PlayerPrefs.SetFloat(MaxHealthSliderKey, HealthPercent);

        // Graphics
        SetBool(FullScreenToggleKey, Fullscreen);
        PlayerPrefs.SetInt(ResolutionDropdownKey, ResolutionIndex);
        PlayerPrefs.SetInt(AntialiasingDropdownKey, AntialiasingIndex);
        PlayerPrefs.SetInt(VSyncDropdownKey, VSyncIndex);
        PlayerPrefs.SetInt(FrameRateDropdownKey, FrameRateIndex);

        // Sound
        PlayerPrefs.SetFloat(MasterSliderKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicSliderKey, MusicVolume);
        PlayerPrefs.SetFloat(FXSliderKey, FXVolume);

        // Controls
    }

    void SetBool(string name, bool value)
    {
        PlayerPrefs.SetInt(name, value ? 1 : 0);
    }

    bool GetBool(string name)
    {
        return PlayerPrefs.GetInt(name) == 1;
    }

    bool GetBool(string name, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(name))
        {
            return GetBool(name);
        }
        return defaultValue;
    }
}
