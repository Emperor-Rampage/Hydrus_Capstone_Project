using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreeAbility : MonoBehaviour
{
    public int Tier { get; set; }
    public int Index { get; set; }
    [SerializeField] Image icon;
    public Image Icon { get { return icon; } }
    [SerializeField] GameObject dimObject;
    public GameObject DimObject {get { return dimObject; } }
    [SerializeField] GameObject highlightObject;
    public GameObject HighlightObject { get { return highlightObject; } }
}
