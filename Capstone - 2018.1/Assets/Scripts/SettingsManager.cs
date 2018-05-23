using System.Collections;
using System.Collections.Generic;
using System.IO;
using AbilityClasses;
using EntityClasses;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class PlayerData
{
    [SerializeField]
    public int classIndex;
    [SerializeField]
    public List<int> abilityTiers;
    [SerializeField]
    public List<int> abilityIndexes;
    [SerializeField]
    public int cores;
}

[System.Serializable]
public class SettingsData
{
    // Gameplay
    public float healthPercent = 1f;

    // Graphics
    public bool fullscreen = true;
    public int resolutionIndex = 0;
    public int textureQualityIndex = 0;
    public int antialiasingIndex = 0;
    public int vSyncIndex = 0;
    public int frameRateIndex = 0;

    // Sound
    public float masterVolume = 1f;
    public float systemVolume = 1f;
    public float musicVolume = 1f;
    public float fxVolume = 1f;
    public float ambientVolume = 1f;

    // Controls
    public KeyCode interactKey = KeyCode.Space;
    public KeyCode turnLeftKey = KeyCode.Q;
    public KeyCode turnRightKey = KeyCode.E;
    public KeyCode ability1Key = KeyCode.Alpha1;
    public KeyCode ability2Key = KeyCode.Alpha2;
    public KeyCode ability3Key = KeyCode.Alpha3;
    public KeyCode ability4Key = KeyCode.Alpha4;
}

public class SettingsManager
{
    const string gameSaveFileName = "gameSave.json";
    const string settingsFileName = "settings.json";
    public SettingsData SettingsData { get; set; } = new SettingsData();
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
    public float HealthPercent { get { return SettingsData.healthPercent; } private set { SettingsData.healthPercent = value; } }
    // Graphics settings
    public bool Fullscreen { get { return SettingsData.fullscreen; } private set { SettingsData.fullscreen = value; } }
    public int ResolutionIndex { get { return SettingsData.resolutionIndex; } private set { SettingsData.resolutionIndex = value; } }
    public int TextureQualityIndex { get { return SettingsData.textureQualityIndex; } private set { SettingsData.textureQualityIndex = value; } }
    public int AntialiasingIndex { get { return SettingsData.antialiasingIndex; } private set { SettingsData.antialiasingIndex = value; } }
    public int VSyncIndex { get { return SettingsData.vSyncIndex; } private set { SettingsData.vSyncIndex = value; } }
    public int FrameRateIndex { get { return SettingsData.frameRateIndex; } private set { SettingsData.frameRateIndex = value; } }
    // Sound settings
    public float MasterVolume { get { return SettingsData.masterVolume; } private set { SettingsData.masterVolume = value; } }
    public float SystemVolume { get { return SettingsData.systemVolume; } private set { SettingsData.systemVolume = value; } }
    public float MusicVolume { get { return SettingsData.musicVolume; } private set { SettingsData.musicVolume = value; } }
    public float FXVolume { get { return SettingsData.fxVolume; } private set { SettingsData.fxVolume = value; } }
    public float AmbientVolume { get { return SettingsData.ambientVolume; } private set { SettingsData.ambientVolume = value; } }
    // Controls settings
    public KeyCode InteractKey { get { return SettingsData.interactKey; } private set { SettingsData.interactKey = value; } }
    public KeyCode TurnLeftKey { get { return SettingsData.turnLeftKey; } private set { SettingsData.turnLeftKey = value; } }
    public KeyCode TurnRightKey { get { return SettingsData.turnRightKey; } private set { SettingsData.turnRightKey = value; } }
    public KeyCode Ability1Key { get { return SettingsData.ability1Key; } private set { SettingsData.ability1Key = value; } }
    public KeyCode Ability2Key { get { return SettingsData.ability2Key; } private set { SettingsData.ability2Key = value; } }
    public KeyCode Ability3Key { get { return SettingsData.ability3Key; } private set { SettingsData.ability3Key = value; } }
    public KeyCode Ability4Key { get { return SettingsData.ability4Key; } private set { SettingsData.ability4Key = value; } }

    public PlayerData LoadGame()
    {
        PlayerData data = null;

        string filePath = Path.Combine(Application.persistentDataPath, gameSaveFileName);

        if (File.Exists(filePath))
        {
            string dataJsonString = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<PlayerData>(dataJsonString);
        }

        return data;
    }

    public void SaveGame(PlayerData data)
    {
        string dataJsonString = JsonUtility.ToJson(data, true);
        // Debug.Log("Saving playerData " + data.classIndex + ", " + data.abilityIndexes + ", " + data.cores + " into " + playerJsonString);

        string filePath = Path.Combine(Application.persistentDataPath, gameSaveFileName);
        File.WriteAllText(filePath, dataJsonString);
    }

    public void SetSettings(UIManager uiManager)
    {
        // If paused, grab the HUD settings stuff.
        // If not pause,d grab the Main Menu settings stuff.
        SettingsContainer container = (uiManager.Paused) ? uiManager.HUDSettings : uiManager.MainMenuSettings;

        // Gameplay

        SettingsData.healthPercent = container.maxHealthSlider.value;
        // Graphics
        SettingsData.fullscreen = container.fullscreenToggle.isOn;
        SettingsData.resolutionIndex = container.resolutionDropdown.value;
        SettingsData.textureQualityIndex = container.textureDropdown.value;
        SettingsData.antialiasingIndex = container.antialiasingDropdown.value;
        SettingsData.vSyncIndex = container.vSyncDropdown.value;
        SettingsData.frameRateIndex = container.frameRateDropdown.value;
        // Sound
        SettingsData.masterVolume = container.masterSlider.value;
        SettingsData.systemVolume = container.systemSlider.value;
        SettingsData.musicVolume = container.musicSlider.value;
        SettingsData.fxVolume = container.fxSlider.value;
        SettingsData.ambientVolume = container.ambientSlider.value;
        // Controls

    }

    public bool LoadSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, settingsFileName);

        if (File.Exists(filePath))
        {
            string dataJsonString = File.ReadAllText(filePath);
            SettingsData = JsonUtility.FromJson<SettingsData>(dataJsonString);
            return true;
        }

        return false;
    }
    public void SaveSettings()
    {
        string dataJsonString = JsonUtility.ToJson(SettingsData, true);

        string filePath = Path.Combine(Application.persistentDataPath, settingsFileName);
        File.WriteAllText(filePath, dataJsonString);
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
