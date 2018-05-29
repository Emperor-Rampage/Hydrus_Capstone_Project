using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextGlow : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] float amount;
    [SerializeField] float speed;
    [SerializeField] float interval;
    [SerializeField] AnimationCurve pulseCurve;
    int glowColorID = ShaderUtilities.ID_GlowColor;

    void Start()
    {
        Material material = GetComponent<TMP_Text>().fontMaterial;
        material.EnableKeyword(ShaderUtilities.Keyword_Glow);
        material.SetColor(glowColorID, color);
        Tween.ShaderFloat(material, "_GlowOuter", 0f, amount, speed, interval, pulseCurve, Tween.LoopType.PingPong, obeyTimescale: false);
    }
}
