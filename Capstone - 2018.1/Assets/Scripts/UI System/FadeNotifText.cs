using System.Collections;
using System.Collections.Generic;
using TMPro;
using Pixelplacement;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FadeNotifText : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float delay;
    [SerializeField] float distance;
    [SerializeField] AnimationCurve curve;

    void Start()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 endPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + distance);
        Tween.AnchoredPosition(rect, endPosition, speed, delay, curve, completeCallback: () => Destroy(gameObject));
        Tween.Value(text.color, Color.clear, (val) => text.color = val, speed, delay, curve);
    }
}
