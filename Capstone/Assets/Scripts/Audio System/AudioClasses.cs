using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioClasses
{
    [System.Serializable]
    public class SoundEffect
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

        private Vector3 position = Vector3.zero;
        public Vector3 Position { get { return position; } private set { position = value; } }

        public SoundEffect(SoundEffect copy)
        {
            Clip = copy.Clip;
            Volume = copy.Volume;
            Pitch = copy.Pitch;
            Position = copy.Position;
        }

        public SoundEffect(SoundEffect copy, Vector3 position)
        {
            Clip = copy.Clip;
            Volume = copy.Volume;
            Pitch = copy.Pitch;
            Position = position;
        }
    }

    [System.Serializable]
    public class BackgroundMusic
    {
        [SerializeField]
        AudioClip audioClip;
        public AudioClip Clip { get { return audioClip; } }

        [SerializeField]
        [Range(0f, 1f)]
        float volume = 1f;
        public float Volume { get { return volume; } }

        [SerializeField]
        [Range(-3f, 3f)]
        float pitch = 1f;
        public float Pitch { get { return pitch; } }
    }
}
