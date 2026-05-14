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
        [SerializeField] private TextMeshProUGUI pageCounterText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;

        private List<Sprite> pages;
        private int currentPageIndex;

        private void OnEnable()
        {
            EventManager.AddListener<OnShowTutorial>(OnShowTutorial);
            nextButton.onClick.AddListener(NextPage);
            prevButton.onClick.AddListener(PreviousPage);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<OnShowTutorial>(OnShowTutorial);
            nextButton.onClick.RemoveListener(NextPage);
            prevButton.onClick.RemoveListener(PreviousPage);
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
            else
            {
                CloseTutorial();
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

            tutorialImage.sprite = pages[currentPageIndex];
            
            if (pageCounterText != null)
            {
                pageCounterText.text = $"Page {currentPageIndex + 1} of {pages.Count}";
            }

            prevButton.gameObject.SetActive(currentPageIndex > 0);
        }

        private void CloseTutorial()
        {
            tutorialPanel.SetActive(false);
            EventManager.Broadcast(new OnChangeGameState(GameState.Playing));
        }
    }
}
