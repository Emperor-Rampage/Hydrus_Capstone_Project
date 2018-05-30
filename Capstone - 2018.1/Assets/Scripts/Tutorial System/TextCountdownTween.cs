using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Pixelplacement;
using System;

[RequireComponent(typeof(TMP_Text))]
public class TextCountdownTween : MonoBehaviour {
    [SerializeField] float start;
    [SerializeField] float end;
    [SerializeField] float speed;
    [SerializeField] float interval;
	void Start () {
		TMP_Text text = GetComponent<TMP_Text>();
		float timeSeconds = start;
		Tween.Value(start, end, (val) => {
	        TimeSpan time = TimeSpan.FromSeconds(val);
			text.text = String.Format("<mspace=2.25em>{0:00}:{1:00}.{2:000}", (int)time.TotalMinutes, time.Seconds, time.Milliseconds);
		}, speed, interval, Tween.EaseLinear, Tween.LoopType.Loop, obeyTimescale: false);
	}
}
