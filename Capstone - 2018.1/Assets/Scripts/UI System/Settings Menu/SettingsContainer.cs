using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsContainer : MonoBehaviour
{
    public TMP_Text tabTitle;
    [Header("Tabs")]
    public Tab[] tabs;
    [Header("Gameplay")]
    public Slider maxHealthSlider;
    public TMP_Text maxHealthText;

    [Header("Graphics")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown textureDropdown;
    public TMP_Dropdown antialiasingDropdown;
    public TMP_Dropdown vSyncDropdown;
    public TMP_Dropdown frameRateDropdown;

    [Header("Sound")]
    public Slider masterSlider;
    public TMP_Text masterValueText;
    public Slider musicSlider;
    public TMP_Text musicValueText;
    public Slider fxSlider;
    public TMP_Text fxValueText;

    [Header("Controls")]

    [Header("Buttons")]
    public Button backButton;
    public Button resetButton;
    public Button applyButton;
}
