using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioClasses
{
    [System.Serializable]
    public class SoundEffect
    {
        [SerializeField]
        public AudioClip audioClip;

        [SerializeField]
        [Range(0f, 1f)]
        public float volume = 1f;

        [SerializeField]
        [Range(-3f, 3f)]
        public float pitch = 1f;

        public Vector3 position;
    }

    [System.Serializable]
    public class BackgroundMusic
    {
        [SerializeField]
        public AudioClip audioClip;
        [SerializeField]
        [Range(0f, 1f)]
        public float volume = 1f;
        [SerializeField]
        [Range(-3f, 3f)]
        public float pitch = 1f;
    }
}
