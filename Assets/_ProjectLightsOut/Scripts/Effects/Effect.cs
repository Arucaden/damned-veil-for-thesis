using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Effects
{
    public class Effect : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float audioDelay = 0f;
        private bool isAudioPlayed;
        private bool isAnimationPlayed;

        private void Awake()
        {
            if (animator != null)
            {
                isAnimationPlayed = true;
            }

            if (audioSource != null)
            {
                StartCoroutine(PlayAudio());
                isAudioPlayed = true;
            }
        }

        private void Start()
        {
            StartCoroutine(WaitUntilAnimationEnds());
            StartCoroutine(WaitUntilAudioEnds());
        }

        private IEnumerator PlayAudio()
        {
            yield return new WaitForSeconds(audioDelay);
            audioSource.Play();
        }

        private void Update()
        {
            if (!isAnimationPlayed && !isAudioPlayed)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator WaitUntilAnimationEnds()
        {
            if (!isAnimationPlayed) yield break;

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            isAnimationPlayed = false;
        }

        private IEnumerator WaitUntilAudioEnds()
        {
            if (!isAudioPlayed) yield break;

            yield return new WaitForSeconds(audioSource.clip.length);
            isAudioPlayed = false;
        }
    }
}
