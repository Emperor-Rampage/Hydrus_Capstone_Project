using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BarTween : MonoBehaviour
{
    [SerializeField] float start;
    [SerializeField] float end;
    [SerializeField] float speed;
    [SerializeField] float interval;
    [SerializeField] AnimationCurve pulseCurve;

    void Start()
    {
        Image image = GetComponent<Image>();

        Tween.Value(start, end, (val) => image.fillAmount = val, speed, interval, pulseCurve, Tween.LoopType.Loop, completeCallback: () => image.fillAmount = 0f, obeyTimescale: false);
    }
}
