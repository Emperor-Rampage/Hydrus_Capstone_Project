using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(Image), typeof(NicerOutline))]
public class ImageGlow : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] float amount;
    [SerializeField] float speed;
    [SerializeField] float interval;
    [SerializeField] AnimationCurve pulseCurve;

    void Start()
    {
        Image image = GetComponent<Image>();
        NicerOutline outline = GetComponent<NicerOutline>();
        Color startColor = new Color(color.r, color.g, color.b, 0f);
        Color endColor = new Color(color.r, color.g, color.b, color.a * amount);
        Tween.Value(startColor, endColor, (val) => outline.effectColor = val, speed, interval, pulseCurve, Tween.LoopType.PingPong, obeyTimescale: false);
    }
}
