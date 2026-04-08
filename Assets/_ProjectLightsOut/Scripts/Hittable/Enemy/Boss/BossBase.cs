using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public interface IBoss
    {
        string CurrentName { get; }
        int Health { get; }
        int MaxHealth { get; }
        Action OnBossDamaged { get; set; }
        Action OnBossHealed { get; set; }
    }

    public abstract class BossBase<T> : Enemy, IBoss where T : BossBase<T>
    {
        [Header("Boss Identity")]
        [SerializeField] private string phase1Name = "Azaleth";
        [SerializeField] private string phase2Name = "Azaleth, The Awakened";

        public string CurrentName => Health <= MaxHealth / 2 ? phase2Name : phase1Name;

        public int MaxHealth { get; set; }
        public Action OnBossDamaged { get; set; }
        public Action OnBossHealed { get; set; }
        public Animator BossAnimator => animator;

        int IBoss.Health => Health;

        private IBossPhase<T> currentPhase;

        public virtual void SetPhase(IBossPhase<T> newPhase)
        {
            currentPhase?.Exit((T)this);
            currentPhase = newPhase;
            currentPhase.Enter((T)this);
        }

        protected override void Start()
        {
            EventManager.Broadcast(new OnBossRegister(this));
            MaxHealth = health;
            SetPhase(CreateEntrancePhase());
        }

        private void Update()
        {
            currentPhase?.UpdatePhase((T)this);
        }

        protected virtual void OnEnable()
        {
            EventManager.AddListener<OnReadyBoss>(HandleReadyBoss);
            EventManager.AddListener<OnEnemyRegister>(HandleEnemyRegister);
            EventManager.AddListener<OnEnemyDead>(HandleEnemyDead);
        }

        protected virtual void OnDisable()
        {
            EventManager.RemoveListener<OnReadyBoss>(HandleReadyBoss);
            EventManager.RemoveListener<OnEnemyRegister>(HandleEnemyRegister);
            EventManager.RemoveListener<OnEnemyDead>(HandleEnemyDead);
        }


        private void HandleReadyBoss(OnReadyBoss e)
        {
            StartCoroutine(ReadyBossSequence());
        }

        protected virtual void HandleEnemyRegister(OnEnemyRegister e) { }
        protected virtual void HandleEnemyDead(OnEnemyDead e) { }


        public override void OnHit(int multiplier, Action OnTargetHit)
        {
            currentPhase?.OnHit((T)this, multiplier, OnTargetHit);
        }

        public void ApplyDamage(int multiplier, Action OnTargetHit)
        {
            health--;
            OnDamaged?.Invoke(multiplier);
            OnTargetHit?.Invoke();
            OnBossDamaged?.Invoke();
        }


        private IEnumerator ReadyBossSequence()
        {
            EventManager.Broadcast(new OnSpotting(transform, 2f));
            yield return new WaitForSeconds(3f);

            EventManager.Broadcast(new OnSpottingEnd(1f));
            EventManager.Broadcast(new OnZoomEnd(1f));
            yield return new WaitForSeconds(1f);

            EventManager.Broadcast(new OnBossReady(this));

            yield return OnEntranceComplete();

            yield return new WaitForSeconds(4.5f);
            EventManager.Broadcast(new OnPlayerEnableShooting(true));
        }

        protected abstract IEnumerator OnEntranceComplete();

        protected abstract IBossPhase<T> CreateEntrancePhase();

        protected abstract IBossPhase<T> CreateDeadPhase();
    }
}
