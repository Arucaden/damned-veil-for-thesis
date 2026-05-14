using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectLightsOut.UI
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private Image tutorialImage;
        [SerializeField] private TextMeshProUGUI pageText;
        [SerializeField] private TextMeshProUGUI pageCounterText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;

        private List<TutorialPageData> pages;
        private int currentPageIndex;

        private void OnEnable()
        {
            EventManager.AddListener<OnShowTutorial>(OnShowTutorial);
            nextButton.onClick.AddListener(NextPage);
            prevButton.onClick.AddListener(PreviousPage);
            closeButton.onClick.AddListener(CloseTutorial);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnShowTutorial>(OnShowTutorial);
            nextButton.onClick.RemoveListener(NextPage);
            prevButton.onClick.RemoveListener(PreviousPage);
            closeButton.onClick.RemoveListener(CloseTutorial);
        }

        private void Start()
        {
            tutorialPanel.SetActive(false);
        }

        private void OnShowTutorial(OnShowTutorial evt)
        {
            pages = evt.Pages;
            currentPageIndex = 0;
            tutorialPanel.SetActive(true);
            UpdateUI();
        }

        private void NextPage()
        {
            if (currentPageIndex < pages.Count - 1)
            {
                currentPageIndex++;
                UpdateUI();
            }
        }

        private void PreviousPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (pages == null || pages.Count == 0) return;

            TutorialPageData current = pages[currentPageIndex];

            tutorialImage.sprite = current.Image;
            tutorialImage.gameObject.SetActive(current.Image != null);

            if (pageText != null)
            {
                pageText.text = current.Text;
            }

            if (pageCounterText != null)
            {
                pageCounterText.text = $"{currentPageIndex + 1}";
            }

            prevButton.gameObject.SetActive(currentPageIndex > 0);
        }

        /// <summary>
        /// Hides the tutorial panel without resuming the game.
        /// Called when the player reaches the last page and presses Next.
        /// </summary>
        private void HideTutorial()
        {
            tutorialPanel.SetActive(false);
        }

        /// <summary>
        /// Hides the tutorial panel AND resumes the game.
        /// Should only be called by the close/exit button.
        /// </summary>
        public void CloseTutorial()
        {
            tutorialPanel.SetActive(false);
            EventManager.Broadcast(new OnChangeGameState(GameState.Playing));
        }
    }
}
