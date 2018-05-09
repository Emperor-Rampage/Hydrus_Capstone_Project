using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundTab : TabContainer
{
    [SerializeField] Slider masterSlider;
    [SerializeField] TMP_Text masterValueText;
    [SerializeField] Slider musicSlider;
    [SerializeField] TMP_Text musicValueText;
    [SerializeField] Slider fxSlider;
    [SerializeField] TMP_Text fxValueText;

    public override void Initialize()
    {
        TabElements[SettingsManager.MasterSliderKey] = masterSlider;
        TabElements[SettingsManager.MasterTextKey] = masterValueText;
        TabElements[SettingsManager.MusicSliderKey] = musicSlider;
        TabElements[SettingsManager.MusicTextKey] = musicValueText;
        TabElements[SettingsManager.FXSliderKey] = fxSlider;
        TabElements[SettingsManager.FXTextKey] = fxValueText;
    }
}
