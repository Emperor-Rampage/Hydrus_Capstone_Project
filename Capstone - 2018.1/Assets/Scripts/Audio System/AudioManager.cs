using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioClasses
{
    public class AudioManager : MonoBehaviour
    {
        [Header("UI Sound Settings")]
        [SerializeField] int maxUISounds;
        [SerializeField] AudioMixerGroup uiAudioMixerGroup;
        Stack<AudioSource> uiSoundPool = new Stack<AudioSource>();

        [Header("Music Settings")]

        [SerializeField]
        const float defaultFadeTime = 0f;

        [Header("Sound Effect Settings")]
        [SerializeField]
        AudioSource soundEffectPrefab;
        [SerializeField]
        AudioSource backgroundMusicPrefab;
        [SerializeField] int maxPoolSize;
        Stack<GameObject> soundEffectPool = new Stack<GameObject>();

        AudioSource musicPlaying;

        // FIXME: Odd bug. Sometimes if the player moves immediately after the cast of an ability is complete,
        //        the sound effect will glitch and only play for a single frame.
        //        Attempts to replicate this bug seemed to indicate that it is random.

        public void PlayUISound(UISound soundEffect)
        {
            if (soundEffect.Clip == null)
            {
                Debug.LogWarning("WARNING: SoundEffect AudioClip is null.");
                return;
            }

            // Get an audio source from the pool, set its clip, volume, and pitch from the soundEffect object.
            // If the pool is empty, create a new source and do the same.
            // Then, play the sound. push into the pool when done.

            AudioSource source;
            if (uiSoundPool.Count > 0)
            {
                source = uiSoundPool.Pop();
                source.clip = soundEffect.Clip;
                source.volume = soundEffect.Volume;
                source.pitch = soundEffect.Pitch;
            }
            else
            {
                source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                source.outputAudioMixerGroup = uiAudioMixerGroup;
                source.clip = soundEffect.Clip;
                source.volume = soundEffect.Volume;
                source.pitch = soundEffect.Pitch;
            }

            source.Play();
            StartCoroutine(AddToUIPool(source, source.clip.length));
        }

        IEnumerator AddToUIPool(AudioSource source, float delay = 0f)
        {
            // Wait the duration of the sound effect, then push it into the pool.
            // If the pool size has exceeded the maximum pool size, pop the excess off the stack and trim the stack.
            yield return new WaitForSeconds(delay);
            // sourceObject.SetActive(false);
            uiSoundPool.Push(source);

            if (uiSoundPool.Count > maxUISounds)
            {
                // Debug.Log("Reached maxPoolSize, trimming excess.");
                for (int i = uiSoundPool.Count; i > maxUISounds; i--)
                {
                    Destroy(uiSoundPool.Pop());
                }
                uiSoundPool.TrimExcess();
            }
        }

        public void FadeInMusic(BackgroundMusic bgMusic, float fadeTime = defaultFadeTime)
        {
            if (bgMusic.Clip == null)
            {
                Debug.LogWarning("WARNING: BackgroundMusic AudioClip is null.");
                return;
            }

            if (musicPlaying != null)
            {
                Debug.LogWarning("WARNING: Attempting to fade in music while music is already playing. Please fade out existing music first.");
                FadeOutMusic(0f);
            }

            GameObject sourceObject = Instantiate(backgroundMusicPrefab.gameObject);
            AudioSource source = sourceObject.GetComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.clip = bgMusic.Clip;
            source.pitch = bgMusic.Pitch;
            source.loop = true;

            source.Play();
            Tween.Volume(source, 0f, bgMusic.Volume, fadeTime, 0f, completeCallback: () => musicPlaying = source);
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
            if (soundEffect.Clip == null)
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
                // Debug.Log("Source found, pulling from pool. Pool now contains " + soundEffectPool.Count);
                sourceObject = soundEffectPool.Pop();
                sourceObject.transform.position = soundEffect.Position;
                source = sourceObject.GetComponent<AudioSource>();
                source.clip = soundEffect.Clip;
                source.volume = soundEffect.Volume;
                source.pitch = soundEffect.Pitch;
                sourceObject.SetActive(true);
            }
            else
            {
                // Debug.Log("Pool is empty, creating new source.");
                sourceObject = Instantiate(soundEffectPrefab.gameObject, soundEffect.Position, Quaternion.identity, this.transform);
                source = sourceObject.GetComponent<AudioSource>();
                source.clip = soundEffect.Clip;
                source.volume = soundEffect.Volume;
                source.pitch = soundEffect.Pitch;
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
                // Debug.Log("Reached maxPoolSize, trimming excess.");
                for (int i = soundEffectPool.Count; i > maxPoolSize; i--)
                {
                    Destroy(soundEffectPool.Pop());
                }
                soundEffectPool.TrimExcess();
            }
        }
    }
}
