using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;

namespace AudioClasses
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Music Settings")]

        [SerializeField]
        const float defaultFadeTime = 0f;

        [Header("Sound Effect Settings")]
        [SerializeField]
        AudioSource soundEffectPrefab;
        [SerializeField] int maxPoolSize;
        Stack<GameObject> soundEffectPool = new Stack<GameObject>();

        AudioSource musicPlaying;

        public void FadeInMusic(BackgroundMusic bgMusic, float fadeTime = defaultFadeTime)
        {
            if (bgMusic.audioClip == null)
            {
                Debug.LogWarning("WARNING: BackgroundMusic AudioClip is null.");
                return;
            }

            if (musicPlaying != null)
            {
                Debug.LogWarning("WARNING: Attempting to fade in music while music is already playing. Please fade out existing music first.");
                FadeOutMusic(0f);
            }

            GameObject sourceObject = Instantiate(soundEffectPrefab.gameObject);
            AudioSource source = sourceObject.GetComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.clip = bgMusic.audioClip;
            source.pitch = bgMusic.pitch;
            source.loop = true;

            source.Play();
            Tween.Volume(source, 0f, bgMusic.volume, fadeTime, 0f, completeCallback: () => musicPlaying = source);
        }

        public void FadeOutMusic(float fadeTime = defaultFadeTime)
        {
            if (musicPlaying == null)
            {
                Debug.LogWarning("WARNING: Attempting to fade out music when no music is playing.");
                return;
            }

            Tween.Volume(musicPlaying, 0f, fadeTime, 0f, completeCallback: () => Destroy(musicPlaying.gameObject));
        }

        public void PlaySoundEffect(SoundEffect soundEffect)
        {
            if (soundEffect.audioClip == null)
            {
                Debug.LogWarning("WARNING: SoundEffect AudioClip is null.");
                return;
            }

            // Get an audio source from the pool, set its clip, volume, pitch, and position from the soundEffect object.
            // If the pool is empty, create a new object and do the same.
            // Then, play the sound. Deactivate and push into the pool when done.

            GameObject sourceObject;
            AudioSource source;
            if (soundEffectPool.Count > 0)
            {
                Debug.Log("Source found, pulling from pool. Pool now contains " + soundEffectPool.Count);
                sourceObject = soundEffectPool.Pop();
                sourceObject.transform.position = soundEffect.position;
                source = sourceObject.GetComponent<AudioSource>();
                source.clip = soundEffect.audioClip;
                source.volume = soundEffect.volume;
                source.pitch = soundEffect.pitch;
                sourceObject.SetActive(true);
            }
            else
            {
                Debug.Log("Pool is empty, creating new source.");
                sourceObject = Instantiate(soundEffectPrefab.gameObject, soundEffect.position, Quaternion.identity);
                source = sourceObject.GetComponent<AudioSource>();
                source.clip = soundEffect.audioClip;
                source.volume = soundEffect.volume;
                source.pitch = soundEffect.pitch;
            }

            source.Play();
            StartCoroutine(AddToPool(sourceObject, source.clip.length));
        }

        IEnumerator AddToPool(GameObject sourceObject, float delay = 0f)
        {
            // Wait the duration of the sound effect, then deactivate it and push it into the pool.
            // If the pool size has exceeded the maximum pool size, pop the excess off the stack and trim the stack.
            yield return new WaitForSeconds(delay);
            sourceObject.SetActive(false);
            soundEffectPool.Push(sourceObject);

            if (soundEffectPool.Count > maxPoolSize)
            {
                Debug.Log("Reached maxPoolSize, trimming excess.");
                for (int i = soundEffectPool.Count; i > maxPoolSize; i--)
                {
                    Destroy(soundEffectPool.Pop());
                }
                soundEffectPool.TrimExcess();
            }
        }
    }
}
