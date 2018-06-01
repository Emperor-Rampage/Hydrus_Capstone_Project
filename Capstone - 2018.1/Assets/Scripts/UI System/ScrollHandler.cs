using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ScrollHandler : MonoBehaviour, IScrollHandler
{
    [Serializable]
    public class ScrollEvent : UnityEvent<PointerEventData> { }

    public ScrollEvent Scroll;
    public void OnScroll(PointerEventData eventData)
    {
        if (Scroll != null)
            Scroll.Invoke(eventData);
    }
}
