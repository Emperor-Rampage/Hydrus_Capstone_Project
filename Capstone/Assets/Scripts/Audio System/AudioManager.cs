using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioClasses
{
    public class AudioManager : MonoBehaviour {
        GameManager manager;
        [SerializeField]
        int maxSounds;
        List<GameObject> soundPool;

        void Start()
        {
            manager = GameManager.Instance;
        }

        public void PlaySound(AudioClip audioClip, Vector3 position)
        {

        }
    }
}
