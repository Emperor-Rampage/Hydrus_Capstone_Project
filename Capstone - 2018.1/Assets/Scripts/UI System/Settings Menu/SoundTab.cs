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

    public void Awake()
    {
        TabElements.Add("MasterSlider", masterSlider);
        TabElements.Add("MasterText", masterValueText);
        TabElements.Add("MusicSlider", musicSlider);
        TabElements.Add("MusicText", musicValueText);
        TabElements.Add("FXSlider", fxSlider);
        TabElements.Add("FXText", fxValueText);
    }
}
