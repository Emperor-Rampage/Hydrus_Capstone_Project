using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectIconContainer : MonoBehaviour
{
    [SerializeField] Image icon;
    public Image Icon { get { return icon; } }
    [SerializeField] Image durationImage;
    public Image DurationImage { get { return durationImage; } }
}
