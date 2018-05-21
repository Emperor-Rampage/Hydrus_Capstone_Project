using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInfoContainer : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    public TMP_Text NameText { get { return nameText; } }
    [SerializeField] TMP_Text costText;
    public TMP_Text CostText { get { return costText; } }
    [SerializeField] TMP_Text typeText;
    public TMP_Text TypeText { get { return typeText; } }
    [SerializeField] TMP_Text tooltipText;
    public TMP_Text TooltipText { get { return tooltipText; } }
    [SerializeField] TMP_Text cooldownText;
    public TMP_Text CooldownText { get { return cooldownText; } }
    [SerializeField] TMP_Text castTimeText;
    public TMP_Text CastTimeText { get { return castTimeText; } }
    [SerializeField] TMP_Text damageText;
    public TMP_Text DamageText { get { return damageText; } }
    [SerializeField] TMP_Text areaText;
    public TMP_Text AreaText { get { return areaText; } }
    [SerializeField] RawImage aoeImage;
    public RawImage AOEImage { get { return aoeImage; } }
}
