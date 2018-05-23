using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New UI Sound", menuName = "Game/Sound/UI Sound")]
public class UISound : ScriptableObject
{
    [SerializeField]
    private AudioClip audioClip;
    public AudioClip Clip { get { return audioClip; } private set { audioClip = value; } }

    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1f;
    public float Volume { get { return volume; } private set { volume = value; } }


    [SerializeField]
    [Range(-3f, 3f)]
    private float pitch = 1f;
    public float Pitch { get { return pitch; } private set { pitch = value; } }
}
