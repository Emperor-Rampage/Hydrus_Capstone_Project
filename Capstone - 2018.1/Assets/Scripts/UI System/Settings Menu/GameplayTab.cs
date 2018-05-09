using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTab : TabContainer
{
    [SerializeField] Slider maxHealthSlider;
    [SerializeField] TMP_Text sliderValueText;

    public override void Initialize()
    {
        Debug.Log("Gameplay tab awake method.");
        TabElements[SettingsManager.MaxHealthSliderKey] = maxHealthSlider;
        TabElements[SettingsManager.MaxHealthTextKey] = sliderValueText;
    }
}
