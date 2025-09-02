using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectLightsOut.DevUtils
{
    public static class SceneLoader
    {
        public static async Task SwitchToAsync(string target)
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

        static Task ToTask(this AsyncOperation op)
        {
            var tcs = new TaskCompletionSource<bool>();
            op.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }
    }
}
