using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LightsOut.UI.Menu
{
    public class LoginUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loginPopupPanel;
        [SerializeField] private TMP_InputField namaInput;
        [SerializeField] private TMP_InputField nimInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button playButton; // The main menu play button

        private void Start()
        {
            // Initial state: Show popup, hide play button
            loginPopupPanel.SetActive(true);
            
            if (playButton != null)
                playButton.gameObject.SetActive(false);

            if (statusText != null)
                statusText.text = "Please enter your Nama and NIM to play.";

            if (submitButton != null)
                submitButton.onClick.AddListener(OnSubmitClicked);
            
            // If already logged in (e.g., coming back to main menu from a level), hide popup
            if (Managers.LoginManager.Instance != null && Managers.LoginManager.Instance.IsLoggedIn)
            {
                loginPopupPanel.SetActive(false);
                if (playButton != null)
                    playButton.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            if (submitButton != null)
                submitButton.onClick.RemoveListener(OnSubmitClicked);
        }

        private void OnSubmitClicked()
        {
            string nama = namaInput != null ? namaInput.text.Trim() : "";
            string nim = nimInput != null ? nimInput.text.Trim() : "";

            // 1. Validation
            if (string.IsNullOrWhiteSpace(nama))
            {
                ShowError("Nama cannot be empty.");
                return;
            }

            if (string.IsNullOrWhiteSpace(nim))
            {
                ShowError("NIM cannot be empty.");
                return;
            }

            if (nim.StartsWith("0"))
            {
                ShowError("NIM cannot start with 0.");
                return;
            }

            if (!long.TryParse(nim, out _))
            {
                ShowError("NIM must be a number.");
                return;
            }

            // 2. Disable UI while processing
            submitButton.interactable = false;
            if (statusText != null)
            {
                statusText.color = Color.white;
                statusText.text = "Checking connection...";
            }

            // 3. Network Check & Submit
            if (Managers.LoginManager.Instance == null)
            {
                ShowError("LoginManager is missing from the scene!");
                submitButton.interactable = true;
                return;
            }

            Managers.LoginManager.Instance.CheckInternetConnection(isConnected =>
            {
                if (!isConnected)
                {
                    ShowError("No internet connection! Please connect and try again.");
                    submitButton.interactable = true;
                    return;
                }

                if (statusText != null)
                {
                    statusText.color = Color.white;
                    statusText.text = "Logging in...";
                }

                Managers.LoginManager.Instance.SubmitActionToGoogleSheet(nama, nim, "Login", (success, response) =>
                {
                    if (success)
                    {
                        // Save session locally for level tracking
                        Managers.LoginManager.Instance.SetUserSession(nama, nim);
                        
                        // Show play button, hide popup
                        if (statusText != null)
                        {
                            statusText.color = Color.green;
                            statusText.text = "Login successful!";
                        }
                        StartCoroutine(ClosePopupRoutine());
                    }
                    else
                    {
                        ShowError("Failed to submit data. Please check your internet and try again.");
                        submitButton.interactable = true;
                    }
                });
            });
        }

        private void ShowError(string message)
        {
            if (statusText != null)
            {
                statusText.color = Color.red;
                statusText.text = message;
            }
        }

        private IEnumerator ClosePopupRoutine()
        {
            yield return new WaitForSeconds(1f);
            
            if (loginPopupPanel != null)
                loginPopupPanel.SetActive(false);
                
            if (playButton != null)
                playButton.gameObject.SetActive(true);
        }
    }
}
