using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsTab : TabContainer
{
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown antialiasingDropdown;
    [SerializeField] TMP_Dropdown vSyncDropdown;
    [SerializeField] TMP_Dropdown frameRateDropdown;

    public override void Initialize()
    {
        TabElements[SettingsManager.FullScreenToggleKey] = fullscreenToggle;
        TabElements[SettingsManager.ResolutionDropdownKey] = resolutionDropdown;
        TabElements[SettingsManager.AntialiasingDropdownKey] = antialiasingDropdown;
        TabElements[SettingsManager.VSyncDropdownKey] = vSyncDropdown;
        TabElements[SettingsManager.FrameRateDropdownKey] = frameRateDropdown;
    }
}
