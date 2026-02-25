using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Non-generic interface for boss UI and external systems
    /// that don't need to know the specific boss type.
    /// </summary>
    public interface IBoss
    {
        int Health { get; }
        int MaxHealth { get; }
        Action OnBossDamaged { get; set; }
        Action OnBossHealed { get; set; }
    }

    /// <summary>
    /// Abstract base for all bosses. Provides a generic state machine,
    /// shared health/damage logic, and the standard boss lifecycle:
    /// Entrance → Phase1 → Transition → Phase2 → Dead.
    /// 
    /// Subclasses implement boss-specific abilities and override
    /// CreateEntrancePhase() and CreateDeadPhase() for their lifecycle.
    /// </summary>
    public abstract class BossBase<T> : Enemy, IBoss where T : BossBase<T>
    {
        public int MaxHealth { get; set; }
        public Action OnBossDamaged { get; set; }
        public Action OnBossHealed { get; set; }
        public Animator BossAnimator => animator;

        // Explicit interface implementation to expose inherited health via IBoss
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

        // --- Event handlers (shared) ---

        private void HandleReadyBoss(OnReadyBoss e)
        {
            StartCoroutine(ReadyBossSequence());
        }

        protected virtual void HandleEnemyRegister(OnEnemyRegister e) { }
        protected virtual void HandleEnemyDead(OnEnemyDead e) { }

        // --- Shared behavior ---

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

        // --- Entrance sequence (shared camera work) ---

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

        // --- Abstract hooks for subclasses ---

        /// <summary>
        /// Called after the shared entrance camera sequence.
        /// Subclass should spawn initial wave, set up abilities, etc.
        /// Return the coroutine for any additional entrance logic.
        /// </summary>
        protected abstract IEnumerator OnEntranceComplete();

        /// <summary>
        /// Create the initial entrance/idle phase for this boss.
        /// </summary>
        protected abstract IBossPhase<T> CreateEntrancePhase();

        /// <summary>
        /// Create the dead phase for this boss.
        /// </summary>
        protected abstract IBossPhase<T> CreateDeadPhase();
    }
}
