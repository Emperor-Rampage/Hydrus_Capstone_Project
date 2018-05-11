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
    public const string TextureDropdownKey = "TextureDropdown";
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
    public int TextureQualityIndex { get; private set; } = 0;
    public int AntialiasingIndex { get; private set; } = 0;
    public int VSyncIndex { get; private set; } = 0;
    public int FrameRateIndex { get; private set; } = 0;
    // Sound settings
    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float FXVolume { get; private set; } = 1f;
    // Controls settings


    public void SetSettings(UIManager uiManager)
    {
        // If paused, grab the HUD settings stuff.
        // If not pause,d grab the Main Menu settings stuff.
        SettingsContainer container = (uiManager.Paused) ? uiManager.HUDSettings : uiManager.MainMenuSettings;

        // Gameplay
        HealthPercent = container.maxHealthSlider.value;
        // Graphics
        Fullscreen = container.fullscreenToggle.isOn;
        ResolutionIndex = container.resolutionDropdown.value;
        TextureQualityIndex = container.textureDropdown.value;
        AntialiasingIndex = container.antialiasingDropdown.value;
        VSyncIndex = container.vSyncDropdown.value;
        FrameRateIndex = container.frameRateDropdown.value;
        // Sound
        MasterVolume = container.masterSlider.value;
        MusicVolume = container.musicSlider.value;
        FXVolume = container.fxSlider.value;
        // Controls
    }
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(FirstPlayKey))
        {
            // Gameplay
            HealthPercent = PlayerPrefs.GetFloat(MaxHealthSliderKey, HealthPercent);

            // Graphics
            Fullscreen = GetBool(FullScreenToggleKey, Fullscreen);
            ResolutionIndex = PlayerPrefs.GetInt(ResolutionDropdownKey, ResolutionIndex);
            TextureQualityIndex = PlayerPrefs.GetInt(TextureDropdownKey, TextureQualityIndex);
            AntialiasingIndex = PlayerPrefs.GetInt(AntialiasingDropdownKey, AntialiasingIndex);
            VSyncIndex = PlayerPrefs.GetInt(VSyncDropdownKey, VSyncIndex);
            FrameRateIndex = PlayerPrefs.GetInt(FrameRateDropdownKey, FrameRateIndex);

            // Sound
            MasterVolume = PlayerPrefs.GetFloat(MasterSliderKey, MasterVolume);
            MusicVolume = PlayerPrefs.GetFloat(MusicSliderKey, MusicVolume);
            FXVolume = PlayerPrefs.GetFloat(FXSliderKey, FXVolume);

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
        PlayerPrefs.SetInt(TextureDropdownKey, TextureQualityIndex);
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
