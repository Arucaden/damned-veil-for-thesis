using ProjectLightsOut.DevUtils;
using UnityEngine;

namespace ProjectLightsOut.Managers
{
    public class OnPlaySFX : GameEvent
    {
        public string AudioName { get; private set; }

        public OnPlaySFX(string audioName)
        {
            AudioName = audioName;
        }
    }

    public class OnPlayBGM : GameEvent
    {
        public string AudioName { get; private set; }
        public AudioClip AudioClip;
        public float FadeIn;

        public OnPlayBGM(string audioName, AudioClip audioClip = null, float fadeIn = 3f)
        {
            AudioName = audioName;
            AudioClip = audioClip;
            FadeIn = fadeIn;
        }
    }

    public class OnStopBGM : GameEvent
    {
        public string AudioName { get; private set; }
        public float FadeOut;

        public OnStopBGM(string audioName, float fadeOut = 3f)
        {
            AudioName = audioName;
            FadeOut = fadeOut;
        }
    }
}