using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.UI
{
    public class LevelSelectionUIManager : MonoBehaviour
    {
        private bool isPressed = false;

        public void OnLevelSelected(string levelName)
        {
            if (isPressed) return;

            isPressed = true;
            AppStateManager.Instance.StartGameplay(levelName);
            EventManager.Broadcast(new OnPlaySFX("Click")); // Or appropriate SFX string
        }

        public void OnBackButtonClicked()
        {
            if (isPressed) return;

            isPressed = true;
            AppStateManager.Instance.GoToMainMenu();
            EventManager.Broadcast(new OnPlaySFX("Click")); // Or appropriate SFX string
        }
    }
}
