using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioClasses
{
    public class SoundEffect
    {
        [SerializeField]
        public AudioClip audioClip;

        [SerializeField]
        [Range(0f, 1f)]
    }
}
