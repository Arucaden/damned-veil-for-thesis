using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using ProjectLightsOut.Managers;
using UnityEngine;

public class OnEnemyChant : GameEvent
{}

public class EnemyChanter : Enemy
{
    [SerializeField] private Animator chantEffectAnimator;
    [SerializeField] private GameObject chantEffect;
    private Coroutine chantCoroutine;

    private void OnEnable()
    {
        OnSpawned += () =>
        {
            Chant();
        };

        EventManager.AddListener<OnTriggerGameOver>(OnGameOver);
        EventManager.AddListener<OnBossDead>(OnBossDead);
    }

    private void OnDisable()
    {
        OnSpawned -= () =>
        {
            Chant();
        };

        EventManager.RemoveListener<OnTriggerGameOver>(OnGameOver);
        EventManager.RemoveListener<OnBossDead>(OnBossDead);
    }

    private void OnBossDead(OnBossDead evt)
    {
        OnHit(1, null);
    }

    private void OnGameOver(OnTriggerGameOver evt)
    {
        StopCoroutine(chantCoroutine);
    }

    protected override void Awake()
    {
        base.Awake();
        chantEffectAnimator.enabled = false;
        chantEffect.SetActive(false);
    }

    private void Chant()
    {
        if (!LevelManager.IsPlayerShootEnabled)
        {
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            return;
        }

        if (!IsHittable) return;

        chantEffectAnimator.enabled = true;
        chantEffect.SetActive(true);
        chantCoroutine = StartCoroutine(Chanting());
    }

    private void OnPlayerEnableShooting(OnPlayerEnableShooting evt)
    {
        if (evt.IsEnabled)
        {
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            if (chantEffectAnimator == null) return;
            chantEffectAnimator.enabled = true;
            chantEffect.SetActive(true);
            chantCoroutine = StartCoroutine(Chanting());
        }
    }

    public override void OnHit(int multiplier, Action OnTargetHit)
    {
        base.OnHit(multiplier, OnTargetHit);
        chantEffectAnimator.enabled = false;
        chantEffect.SetActive(false);
        shadowRenderer.enabled = false;

        OnSpawned -= () =>
        {
            Chant();
        };

        if (chantCoroutine == null) return;
        StopCoroutine(chantCoroutine);
    }

    private IEnumerator Chanting()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.65f);
            EventManager.Broadcast(new OnEnemyChant());
            yield return new WaitForSeconds(0.65f);
        }
    }
}
