using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTab : TabContainer
{
    [SerializeField] Slider maxHealthSlider;
    [SerializeField] TMP_Text sliderValueText;

    public void Awake()
    {
        TabElements.Add("MaxHealthSlider", maxHealthSlider);
        TabElements.Add("SliderValueText", sliderValueText);
    }
}
