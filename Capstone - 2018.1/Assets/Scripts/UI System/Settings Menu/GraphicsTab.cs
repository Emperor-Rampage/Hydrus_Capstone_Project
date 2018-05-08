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

    public void Awake()
    {
        TabElements.Add("FullscreenText", fullscreenToggle);
        TabElements.Add("ResolutionDropdown", resolutionDropdown);
        TabElements.Add("AntialiasingDropdown", antialiasingDropdown);
        TabElements.Add("VSyncDropdown", vSyncDropdown);
        TabElements.Add("FrameRateDropdown", frameRateDropdown);
    }
}
