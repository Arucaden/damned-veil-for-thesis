using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectLightsOut.DevUtils
{
    public static class SceneLoader
    {
        [System.Serializable]
        public class FadeSettings
        {
            [Tooltip("Duration of fade in (to black) in seconds")]
            public float fadeInDuration = 0.5f;
            
            [Tooltip("Duration of fade out (from black) in seconds")]
            public float fadeOutDuration = 0.5f;
            
            [Tooltip("Minimum time to show loading screen in seconds (adds padding if loading is too fast)")]
            public float minLoadingTime = 1.0f;
            
            [Tooltip("Additional delay after fade out completes")]
            public float postLoadDelay = 0.2f;
        }

        private static FadeSettings _settings = new FadeSettings();
        private static CanvasGroup _fadeCanvasGroup;

        public static void Initialize(FadeSettings settings, CanvasGroup fadeCanvasGroup)
        {
            _settings = settings;
            _fadeCanvasGroup = fadeCanvasGroup;
        }

        public static async Task SwitchToAsync(string target)
        {
            if (_fadeCanvasGroup == null)
            {
                Debug.LogWarning("FadeCanvasGroup not initialized. Using instant loading.");
                await SwitchToAsyncInstant(target);
                return;
            }

            float startTime = Time.time;
            
            // Fade in to black
            await FadeToBlack(_settings.fadeInDuration);
            
            // Show loading screen
            LoadingScreen.Instance.Show();
            
            // Unload all scenes except the persistent one (buildIndex 0)
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.buildIndex != 0)
                    await SceneManager.UnloadSceneAsync(s).ToTask();
            }
            
            // Load the new scene
            await SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive).ToTask();
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(target));
            
            // Calculate elapsed time and add padding if needed
            float elapsedTime = Time.time - startTime;
            float remainingTime = _settings.minLoadingTime - elapsedTime;
            
            if (remainingTime > 0)
            {
                await Task.Delay((int)(remainingTime * 1000));
            }
            
            // Hide loading screen and fade out from black
            LoadingScreen.Instance.Hide();
            await FadeFromBlack(_settings.fadeOutDuration);
            
            // Additional delay after fade out
            if (_settings.postLoadDelay > 0)
            {
                await Task.Delay((int)(_settings.postLoadDelay * 1000));
            }
        }

        private static async Task SwitchToAsyncInstant(string target)
        {
            LoadingScreen.Instance.Show();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.buildIndex != 0)
                    await SceneManager.UnloadSceneAsync(s).ToTask();
            }
            await SceneManager.LoadSceneAsync(target, LoadSceneMode.Additive).ToTask();

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(target));
            
            LoadingScreen.Instance.Hide();
        }

        public static async Task FadeToBlack(float duration)
        {
            if (duration <= 0)
            {
                _fadeCanvasGroup.alpha = 1;
                _fadeCanvasGroup.interactable = true;
                _fadeCanvasGroup.blocksRaycasts = true;
                return;
            }

            _fadeCanvasGroup.interactable = true;
            _fadeCanvasGroup.blocksRaycasts = true;

            float timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                timeElapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = Mathf.Clamp01(timeElapsed / duration);
                await Task.Yield();
            }
            _fadeCanvasGroup.alpha = 1;
        }

        public static async Task FadeFromBlack(float duration)
        {
            if (duration <= 0)
            {
                _fadeCanvasGroup.alpha = 0;
                _fadeCanvasGroup.interactable = false;
                _fadeCanvasGroup.blocksRaycasts = false;
                return;
            }

            float timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                timeElapsed += Time.unscaledDeltaTime;
                _fadeCanvasGroup.alpha = 1 - Mathf.Clamp01(timeElapsed / duration);
                await Task.Yield();
            }
            _fadeCanvasGroup.alpha = 0;
            _fadeCanvasGroup.interactable = false;
            _fadeCanvasGroup.blocksRaycasts = false;
        }

        static Task ToTask(this AsyncOperation op)
        {
            var tcs = new TaskCompletionSource<bool>();
            op.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
