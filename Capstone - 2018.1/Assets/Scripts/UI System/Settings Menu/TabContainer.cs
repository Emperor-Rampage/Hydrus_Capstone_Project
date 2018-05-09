using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TabContainer : MonoBehaviour
{
    protected Dictionary<string, object> TabElements { get; } = new Dictionary<string, object>();
    public T GetTabElement<T>(string key) where T : Selectable
    {
        Debug.Log("Getting element " + key + " .. count is " + TabElements.Count);
        if (TabElements.ContainsKey(key))
        {
            object element = TabElements[key];
            return element as T;
        }
        return default(T);
    }

    public virtual void Initialize() { }
}
