using ProjectLightsOut.DevUtils;
using UnityEngine;
using ProjectLightsOut.Managers;
using System.Collections;

public class HUDBulletUI : MonoBehaviour
{
    [SerializeField] private Transform bulletParent;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Vector2 retractPosition;
    [SerializeField] private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = retractPosition;
    }
    private void OnEnable()
    {
        EventManager.AddListener<OnBulletReload>(OnBulletReload);
        EventManager.AddListener<OnProjectileShoot>(OnProjectileShoot);
        EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnBulletReload>(OnBulletReload);
        EventManager.RemoveListener<OnProjectileShoot>(OnProjectileShoot);
        EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
    }

    private void OnPlayerEnableShooting(OnPlayerEnableShooting e)
    {
        if (e.IsEnabled)
        {
            StartCoroutine(Extend());
        }

        else
        {
            StartCoroutine(Retract());
        }
    }

    private void OnBulletReload(OnBulletReload e)
    {
        for (int i = 0; i < e.Bullets; i++)
        {
            Instantiate(bulletPrefab, bulletParent);
        }
    }

    private void OnProjectileShoot(OnProjectileShoot e)
    {
        Destroy(bulletParent.GetChild(bulletParent.childCount - 1).gameObject);
    }

    private IEnumerator Retract()
    {
        float time = 0;
        float duration = 0.5f;
        canvasGroup.alpha = 1;

        Vector2 currentPos = rectTransform.anchoredPosition;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, retractPosition, time / duration);
            canvasGroup.alpha = Mathf.Lerp(1, 0, time / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator Extend()
    {
        float time = 0;
        float duration = 0.5f;
        Vector2 currentPos = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(currentPos, originalPosition, time / duration);
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;
    }
}
