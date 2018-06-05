using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDAbility : MonoBehaviour
{
    [SerializeField] Image icon;
    public Image Icon { get { return icon; } }

    [SerializeField] Image cooldownTimer;
    public Image CooldownTimer { get { return cooldownTimer; } }

    [SerializeField] TMP_Text cooldownText;
    public TMP_Text CooldownText { get { return cooldownText; } }
    [SerializeField] Image castTimer;
    public Image CastTimer { get { return castTimer; } }
    [SerializeField] TMP_Text keyText;
    public TMP_Text KeyText { get { return keyText; } }
}
