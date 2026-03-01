#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectLightsOut.DevUtils
{
    public static class BootLoader
    {
        private const string BOOTSTRAP_SCENE_NAME = "00_BootStrap";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeBootstrap()
        {
            if (SceneManager.GetActiveScene().name == BOOTSTRAP_SCENE_NAME) return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == BOOTSTRAP_SCENE_NAME)
                    return;
            }

            SceneManager.LoadScene(BOOTSTRAP_SCENE_NAME, LoadSceneMode.Additive);
        }
    }
}
#endif
