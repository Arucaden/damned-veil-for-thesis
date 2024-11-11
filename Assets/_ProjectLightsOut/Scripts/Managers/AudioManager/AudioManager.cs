using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    [Serializable] public class AudioData
    {
        public string name;
        public AudioClip clip;
        public AudioSource audioSource;
    }

    public class AudioManager : Singleton<AudioManager>
    {
        private struct AudioSourceCoroutine
        {
            public Coroutine coroutine;
            public AudioSource audioSource;
        }

        [SerializeField] private List<AudioData> audioData = new List<AudioData>();
        private List<AudioSourceCoroutine> audioSourceCoroutines = new List<AudioSourceCoroutine>();
        private Action OnFadeOutComplete;

        protected override void Awake()
        {
            base.Awake(); 

            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            EventManager.AddListener<OnPlaySFX>(OnPlayAudio);
            EventManager.AddListener<OnPlayBGM>(OnPlayBGM);
            EventManager.AddListener<OnStopBGM>(OnStopBGM);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnPlaySFX>(OnPlayAudio);
            EventManager.RemoveListener<OnPlayBGM>(OnPlayBGM);
            EventManager.RemoveListener<OnStopBGM>(OnStopBGM);
        }

        private void OnPlayAudio(OnPlaySFX evt)
        {
            var data = audioData.Find(x => x.name.ToLower() == evt.AudioName.ToLower());

            if (data == null)
            {
                Debug.LogError($"AudioManager: {evt.AudioName} not found.");
                return;
            }

            //AudioClip clip = data.clip;

            // if (data.clip != null)
            // {
            //     data.audioSource.clip = data.clip;
            // }

            // else
            // {
            //     //Debug.Log("AudioManager: AudioClip is null. Using default AudioClip.");
            // }

            data.audioSource.Play();
            //data.audioSource.clip = clip;
        }

        private void OnStopBGM(OnStopBGM evt)
        {
            AudioData data = audioData.Find(x => x.name.ToLower() == evt.AudioName.ToLower());

            if (data == null)
            {
                Debug.LogError($"AudioManager: {evt.AudioName} not found.");
                return;
            }

            if (evt.FadeOut > 0)
            {
                Coroutine coroutine = StartCoroutine(FadeOut(data.audioSource, evt.FadeOut));
                PlayCoroutine(data.audioSource, coroutine);
            }

            else
            {
                data.audioSource.Stop();
            }
        }

        private void OnPlayBGM(OnPlayBGM evt)
        {
            AudioData data = audioData.Find(x => x.name.ToLower() == evt.AudioName.ToLower());

            if (data == null)
            {
                Debug.LogError($"AudioManager: {evt.AudioName} not found.");
                return;
            }

            if (data.audioSource.isPlaying)
            {
                Coroutine coroutine = StartCoroutine(FadeOut(data.audioSource, 3f));
                PlayCoroutine(data.audioSource, coroutine);

                OnFadeOutComplete = () => PlayBGM(data, evt.FadeIn);
            }

            else
            {
                PlayBGM(data, evt.FadeIn);
            }
        }

        private void PlayBGM(AudioData data, float fadeIn)
        {
            OnFadeOutComplete = null;
            
            if (data.clip != null)
            {
                data.audioSource.clip = data.clip;
            }

            else
            {
                //Debug.Log("AudioManager: AudioClip is null. Using default AudioClip.");
            }
                
            if (fadeIn > 0)
            {
                Coroutine coroutine = StartCoroutine(FadeIn(data.audioSource, fadeIn));
                PlayCoroutine(data.audioSource, coroutine);
            }

            else
            {
                data.audioSource.Play();
            }
        }

        private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
        {
            float startVolume = audioSource.volume;

            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
            OnFadeOutComplete?.Invoke();
        }

        private IEnumerator FadeIn(AudioSource audioSource, float fadeTime)
        {
            float startVolume = audioSource.volume;
            audioSource.volume = 0;
            audioSource.Play();

            while (audioSource.volume < startVolume)
            {
                audioSource.volume += startVolume * Time.deltaTime / fadeTime;

                yield return null;
            }

            audioSource.volume = startVolume;
        }

        private void PlayCoroutine(AudioSource audioSource, Coroutine coroutine)
        {
            AudioSourceCoroutine coroutineData = audioSourceCoroutines.Find(x => x.audioSource == audioSource);

            if (coroutineData.coroutine != null)
            {
                StopCoroutine(coroutineData.coroutine);
                coroutineData.coroutine = coroutine;
            }

            else
            {
                AudioSourceCoroutine data = new AudioSourceCoroutine
                {
                    audioSource = audioSource,
                    coroutine = coroutine
                };

                audioSourceCoroutines.Add(data);
            }
        }
    }
}