using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LightsOut.Managers
{
    [Serializable]
    public class CachedAction
    {
        public string Nama;
        public string NIM;
        public string Action;
    }

    [Serializable]
    public class CachedActionList
    {
        public List<CachedAction> Actions = new List<CachedAction>();
    }

    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance { get; private set; }

        [Header("Backend Setup")]
        [Tooltip("The Web App URL from Google Apps Script")]
        [SerializeField] private string webAppUrl = "PASTE_YOUR_URL_HERE";
        
        [Header("Testing/Debug")]
        [SerializeField] private bool disableNetworkCheck = false;

        public string CurrentUserNama { get; private set; }
        public string CurrentUserNIM { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUserNama) && !string.IsNullOrEmpty(CurrentUserNIM);

        private const string CACHE_KEY = "LightsOut_CachedActions";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // On startup, try to send any cached actions
            if (HasCachedActions())
            {
                StartCoroutine(TrySendCachedActionsRoutine());
            }
        }

        public void SetUserSession(string nama, string nim)
        {
            CurrentUserNama = nama;
            CurrentUserNIM = nim;
        }

        /// <summary>
        /// Submits an action to the Google Sheet. Handles offline caching automatically if it fails.
        /// </summary>
        public void SubmitActionToGoogleSheet(string nama, string nim, string action, Action<bool, string> onComplete)
        {
            StartCoroutine(SubmitRoutine(nama, nim, action, onComplete));
        }

        /// <summary>
        /// Easy wrapper to track a level using the current logged-in user.
        /// </summary>
        public void TrackLevelComplete(string levelName)
        {
            if (!IsLoggedIn)
            {
                Debug.LogWarning("Cannot track level, user is not logged in!");
                return;
            }
            SubmitActionToGoogleSheet(CurrentUserNama, CurrentUserNIM, levelName, null);
        }

        public void CheckInternetConnection(Action<bool> onComplete)
        {
            if (disableNetworkCheck)
            {
                onComplete?.Invoke(true);
                return;
            }
            StartCoroutine(CheckInternetRoutine(onComplete));
        }

        private IEnumerator CheckInternetRoutine(Action<bool> onComplete)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            // Ping Google or the webAppUrl to be absolutely sure
            using (UnityWebRequest req = UnityWebRequest.Get("https://www.google.com"))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
                {
                    onComplete?.Invoke(false);
                }
                else
                {
                    onComplete?.Invoke(true);
                }
            }
        }

        private IEnumerator SubmitRoutine(string nama, string nim, string action, Action<bool, string> onComplete)
        {
            WWWForm form = new WWWForm();
            form.AddField("nama", nama);
            form.AddField("nim", nim);
            form.AddField("action", action);

            using (UnityWebRequest req = UnityWebRequest.Post(webAppUrl, form))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"[LoginManager] Failed to send action '{action}'. Caching locally...");
                    
                    // Only cache non-login actions. We don't want to cache failed logins since they block entry.
                    if (action != "Login")
                    {
                        CacheAction(nama, nim, action);
                    }
                    
                    onComplete?.Invoke(false, "Connection Error. Data saved locally.");
                }
                else
                {
                    Debug.Log($"[LoginManager] Action '{action}' sent successfully!");
                    onComplete?.Invoke(true, req.downloadHandler.text);
                    
                    // If we successfully sent, let's also try to flush the cache
                    if (HasCachedActions())
                    {
                        StartCoroutine(TrySendCachedActionsRoutine());
                    }
                }
            }
        }

        #region Offline Caching

        private bool HasCachedActions()
        {
            return PlayerPrefs.HasKey(CACHE_KEY) && !string.IsNullOrEmpty(PlayerPrefs.GetString(CACHE_KEY));
        }

        private void CacheAction(string nama, string nim, string action)
        {
            CachedActionList list = GetCachedList();
            list.Actions.Add(new CachedAction { Nama = nama, NIM = nim, Action = action });
            
            string json = JsonUtility.ToJson(list);
            PlayerPrefs.SetString(CACHE_KEY, json);
            PlayerPrefs.Save();
        }

        private CachedActionList GetCachedList()
        {
            if (PlayerPrefs.HasKey(CACHE_KEY))
            {
                string json = PlayerPrefs.GetString(CACHE_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonUtility.FromJson<CachedActionList>(json);
                }
            }
            return new CachedActionList();
        }

        private IEnumerator TrySendCachedActionsRoutine()
        {
            CachedActionList list = GetCachedList();
            if (list.Actions.Count == 0) yield break;

            Debug.Log($"[LoginManager] Attempting to resend {list.Actions.Count} cached actions...");

            // Try to ping internet first
            bool isOnline = false;
            yield return StartCoroutine(CheckInternetRoutine(result => isOnline = result));

            if (!isOnline)
            {
                Debug.Log("[LoginManager] Still offline. Will try again later.");
                yield break;
            }

            // Keep track of actions that fail so we can put them back in the cache
            List<CachedAction> failedActions = new List<CachedAction>();

            foreach (var cachedAction in list.Actions)
            {
                WWWForm form = new WWWForm();
                form.AddField("nama", cachedAction.Nama);
                form.AddField("nim", cachedAction.NIM);
                form.AddField("action", cachedAction.Action);

                using (UnityWebRequest req = UnityWebRequest.Post(webAppUrl, form))
                {
                    yield return req.SendWebRequest();

                    if (req.result == UnityWebRequest.Result.ConnectionError || req.result == UnityWebRequest.Result.ProtocolError)
                    {
                        failedActions.Add(cachedAction); // Failed again, keep it
                    }
                }
            }

            // Update cache
            if (failedActions.Count == 0)
            {
                PlayerPrefs.DeleteKey(CACHE_KEY);
                Debug.Log("[LoginManager] All cached actions sent successfully!");
            }
            else
            {
                list.Actions = failedActions;
                PlayerPrefs.SetString(CACHE_KEY, JsonUtility.ToJson(list));
                Debug.Log($"[LoginManager] Sent some actions, but {failedActions.Count} still failed.");
            }
            PlayerPrefs.Save();
        }

        #endregion
    }
}
