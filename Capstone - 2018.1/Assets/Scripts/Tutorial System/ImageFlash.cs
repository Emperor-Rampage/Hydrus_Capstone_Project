using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pixelplacement;

[RequireComponent(typeof(Image))]
public class ImageFlash : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float interval;
    void Start()
    {
        float v = 0f;
        Image image = GetComponent<Image>();
        Color targetColor = new Color(image.color.r, image.color.g, image.color.b, 1f);
        Tween.Value(0f, 1f, (val) => v = val, interval, 0f, Tween.EaseLinear, Tween.LoopType.Loop, completeCallback: () =>
        {
            Tween.Color(image, targetColor, speed, 0f, Tween.EaseWobble, obeyTimescale: false);
        }, obeyTimescale: false);
    }
}
