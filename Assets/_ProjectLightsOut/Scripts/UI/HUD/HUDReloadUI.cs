using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using TMPro;
using UnityEngine;

namespace ProjectLightsOut.UI
{
    public class HUDReloadUI : MonoBehaviour
    {
    private int bullets;
    private int currentBullets;
    private bool reloading;

    [SerializeField] private TextMeshProUGUI reloadText;

    private void OnEnable()
    {
        EventManager.AddListener<OnGrantReload>(OnGrantReload);
        EventManager.AddListener<OnBulletReload>(OnBulletReload);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnGrantReload>(OnGrantReload);
        EventManager.RemoveListener<OnBulletReload>(OnBulletReload);
    }

    private void OnGrantReload(OnGrantReload e)
    {
        bullets = e.Bullets;
        currentBullets = 0;
        reloading = true;
    }

    private void OnBulletReload(OnBulletReload e)
    {
        currentBullets += e.Bullets;
        if (currentBullets >= bullets)
        {
            reloading = false;
        }
    }

    private void Update()
    {
        if (!reloading)
        {
            reloadText.alpha = 0;
            return;
        }

        reloadText.alpha = 0.2f + Mathf.PingPong(Time.time, 0.4f);
    }
    }
}
