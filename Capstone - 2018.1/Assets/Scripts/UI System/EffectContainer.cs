using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EffectContainer : MonoBehaviour
{
    [SerializeField] Image icon;
    public Image Icon { get { return icon; } }
    [SerializeField] TMP_Text effectText;
    public TMP_Text EffectText { get { return effectText; } }
}
