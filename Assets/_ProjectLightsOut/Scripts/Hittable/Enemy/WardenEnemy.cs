using UnityEngine;
using ProjectLightsOut.Managers;
using ProjectLightsOut.DevUtils;
using System.Collections.Generic;

namespace ProjectLightsOut.Gameplay
{
    public class WardenEnemy : Enemy
    {
        [Header("Warden Settings")]
        [Tooltip("The color applied to other enemies while they are shielded by this Warden.")]
        [SerializeField] private Color shieldColor = new Color(0.8f, 0.8f, 1f, 0.8f);
        
        private WaveManager waveManager;
        private List<Enemy> shieldedEnemies = new List<Enemy>();

        protected override void Awake()
        {
            base.Awake();
            waveManager = FindObjectOfType<WaveManager>();
        }

        protected override void Start()
        {
            base.Start();
            EventManager.AddListener<OnEnemyRegister>(OnEnemyRegistered);
            ApplyShieldsToExistingEnemies();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<OnEnemyRegister>(OnEnemyRegistered);
            RemoveShields();
        }

        public override void OnHit(int multiplier, System.Action OnTargetHit)
        {
            base.OnHit(multiplier, OnTargetHit);
            
            if (health <= 0)
            {
                RemoveShields();
                
#if UNITY_EDITOR || UNITY_STANDALONE
                var light2D = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
                if (light2D != null)
                {
                    light2D.enabled = false;
                }
#endif
            }
        }

        private void ApplyShieldsToExistingEnemies()
        {
            if (waveManager == null) return;

            foreach (var enemy in waveManager.Enemies)
            {
                ShieldEnemy(enemy);
            }
        }

        private void OnEnemyRegistered(OnEnemyRegister evt)
        {
            ShieldEnemy(evt.Enemy);
        }

        private void ShieldEnemy(Enemy enemy)
        {
            if (enemy == null || enemy == this) return;
            
            if (enemy is WardenEnemy) return;

            if (!shieldedEnemies.Contains(enemy))
            {
                shieldedEnemies.Add(enemy);
                enemy.SetWardenShield(true, shieldColor);
            }
        }

        private void RemoveShields()
        {
            foreach (var enemy in shieldedEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetWardenShield(false);
                }
            }
            shieldedEnemies.Clear();
            
            EventManager.RemoveListener<OnEnemyRegister>(OnEnemyRegistered);
        }
    }
}
