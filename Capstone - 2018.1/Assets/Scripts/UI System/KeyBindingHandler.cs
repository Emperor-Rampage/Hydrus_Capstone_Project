using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class KeyBindingHandler : MonoBehaviour, IPointerClickHandler
{
	[Serializable]
	public class ClickEvent : UnityEvent<PointerEventData> { }

	public KeyCode Value { get; set; }
	[SerializeField] TMP_Text displayText;
	public TMP_Text DisplayText { get { return displayText; } }
	[SerializeField] TMP_Text buttonText;
	public TMP_Text ButtonText { get { return buttonText; } }

	public ClickEvent Click;
    public void OnPointerClick(PointerEventData eventData)
    {
		if (Click != null)
			Click.Invoke(eventData);
    }
}
