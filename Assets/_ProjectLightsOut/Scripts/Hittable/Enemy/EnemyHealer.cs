using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Gameplay;
using ProjectLightsOut.Managers;
using UnityEngine;

public class EnemyHealer : Enemy
{
    [SerializeField] private GameObject thread;
    [SerializeField] protected Animator chantEffectAnimator;
    [SerializeField] private GameObject chantEffect;
    private List<BossBuffThread> threads = new List<BossBuffThread>();
    private Coroutine chantCoroutine;

    protected override void Awake()
    {
        base.Awake();
        chantEffectAnimator.enabled = false;
        chantEffect.SetActive(false);
    }

    private void OnEnable()
    {
        OnSpawned += () =>
        {
            Buff();
        };

        EventManager.AddListener<OnBossDead>(OnBossDead);
    }

    private void OnDisable()
    {
        OnSpawned -= () =>
        {
            Buff();
        };



        EventManager.RemoveListener<OnBossDead>(OnBossDead);
        EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
    }

    private void OnPlayerEnableShooting(OnPlayerEnableShooting evt)
    {
        if (evt.IsEnabled)
        {
            EventManager.RemoveListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            chantEffectAnimator.enabled = true;
            chantEffect.SetActive(true);
            chantCoroutine = StartCoroutine(Buffing());
        }
    }

    protected virtual void Buff()
    {
        if (!LevelManager.IsPlayerShootEnabled)
        {
            EventManager.AddListener<OnPlayerEnableShooting>(OnPlayerEnableShooting);
            return;
        }

        if (!IsHittable) return;

        chantEffectAnimator.enabled = true;
        chantEffect.SetActive(true);
        chantCoroutine = StartCoroutine(Buffing());
    }

    private void OnBossDead(OnBossDead evt)
    {
        foreach (var thread in threads)
        {
            if (thread == null) continue;
            Destroy(thread.gameObject);
        }

        OnHit(1, null);
    }

    public override void OnHit(int multiplier, Action OnTargetHit)
    {
        base.OnHit(multiplier, OnTargetHit);
        chantEffectAnimator.enabled = false;
        chantEffect.SetActive(false);
        shadowRenderer.enabled = false;

        OnSpawned -= () =>
        {
            Buff();
        };

        if (chantCoroutine == null) return;
        StopCoroutine(chantCoroutine);
    }

    protected virtual IEnumerator Buffing()
    {
        while (true)
        {
            chantEffectAnimator.SetTrigger("Buff");
            //EventManager.Broadcast(new OnPlaySFX("ChannelHealth")); 
            BossBuffThread buffThread = Instantiate(thread, transform.position, Quaternion.identity).GetComponent<BossBuffThread>();
            threads.Add(buffThread);
            buffThread.SetTarget(LevelManager.BossTransform);

            buffThread.OnThreadDestroyed += () =>
            {
                threads.Remove(buffThread);
            };

            yield return new WaitForSeconds(3f);
        }
    }
}
