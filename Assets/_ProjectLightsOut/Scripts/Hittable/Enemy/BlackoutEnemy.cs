using System;
using System.Collections;
using ProjectLightsOut.DevUtils;
using ProjectLightsOut.Managers;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class BlackoutEnemy : Enemy
    {
        [Header("Blackout Ability Settings")]
        [Tooltip("How many times the light flickers before total blackout")]
        [SerializeField] private int flickerCount = 3;
        
        [Tooltip("Time spent dark and bright during a flicker")]
        [SerializeField] private float flickerInterval = 0.2f;

        [Tooltip("How long the final fade to black takes")]
        [SerializeField] private float fadeTransitionTime = 1f;

        protected override void Start()
        {
            base.Start();
            
            OnSpawned += TriggerBlackoutAbility;
        }

        private void OnDestroy()
        {
            OnSpawned -= TriggerBlackoutAbility;
            EventManager.Broadcast(new OnToggleBlackout(false, 0.2f));
        }

        private void TriggerBlackoutAbility()
        {
            StartCoroutine(BlackoutRoutine());
        }

        private IEnumerator BlackoutRoutine()
        {
            // Wait a moment after spawning before starting the ability
            yield return new WaitForSeconds(1f);
            
            Debug.Log("[BlackoutEnemy] Ability started! Flickering lights...");

            // Flicker phase
            for (int i = 0; i < flickerCount; i++)
            {
                // Flicker dark
                EventManager.Broadcast(new OnToggleBlackout(true, 0.05f));
                yield return new WaitForSeconds(flickerInterval);
                
                // Restore light
                EventManager.Broadcast(new OnToggleBlackout(false, 0.05f));
                yield return new WaitForSeconds(flickerInterval);
            }

            // Optional slight pause before the final drop
            yield return new WaitForSeconds(0.25f);

            Debug.Log("[BlackoutEnemy] Screen going dark permanently...");
            
            // Final deep blackout
            EventManager.Broadcast(new OnToggleBlackout(true, fadeTransitionTime));
            
            // Note: The darkness now remains indefinitely until the enemy dies.
        }

        public override void OnHit(int multiplier, Action OnTargetHit)
        {
            base.OnHit(multiplier, OnTargetHit);

            // 1. When he dies, the dark cloud should be gone
            if (Health <= 0)
            {
                StopAllCoroutines(); 
                
                // Swiftly restore light since the enemy died. This clears the dark cloud.
                Debug.Log("[BlackoutEnemy] Enemy caster died! Dark cloud is fading away immediately...");
                EventManager.Broadcast(new OnToggleBlackout(false, 0.2f));
            }
        }
    }
}
