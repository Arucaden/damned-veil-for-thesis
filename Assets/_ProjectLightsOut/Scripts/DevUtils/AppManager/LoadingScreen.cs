using UnityEngine;
using UnityEngine.UI;

namespace ProjectLightsOut.DevUtils
{
    public class LoadingScreen : Singleton<LoadingScreen>
    {
        [SerializeField] private GameObject _loadingScreen;
        protected override void Awake()
        {
            base.Awake();

            Hide();
        }

        public void Show()
        {
            _loadingScreen.SetActive(true);
        }

        public void Hide()
        {
            _loadingScreen.SetActive(false);
        }
    }
}
