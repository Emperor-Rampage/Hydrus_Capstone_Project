using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tab : MonoBehaviour
{
    [SerializeField] string title;
    public string Title { get { return title; } }
    public new GameObject gameObject;
}
