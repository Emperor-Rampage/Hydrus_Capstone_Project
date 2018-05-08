using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class TabContainer : MonoBehaviour
{
    protected Dictionary<string, object> TabElements { get; } = new Dictionary<string, object>();
    public T GetTabElement<T>(string key) where T : class
    {
        if (TabElements.ContainsKey(key))
        {
            object element = TabElements[key];
            return element as T;
        }
        return default(T);
    }
}
