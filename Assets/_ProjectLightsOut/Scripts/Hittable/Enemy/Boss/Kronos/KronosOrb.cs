using System;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    /// <summary>
    /// Destructible orbiting object that protects Kronos.
    /// Implements IHittable so bullets interact with it normally.
    /// When destroyed, notifies the parent KronosTimeShield.
    /// </summary>
    public class KronosOrb : MonoBehaviour, IHittable
    {
        [SerializeField] private int health = 1;
        [SerializeField] private GameObject destroyEffect;

        private bool isHittable = true;
        public bool IsHittable => isHittable;

        public Action OnOrbDestroyed;

        public void OnHit(int multiplier, Action onDeadAction = null)
        {
            health--;
            onDeadAction?.Invoke();

            if (health <= 0)
            {
                isHittable = false;

                if (destroyEffect != null)
                {
                    Instantiate(destroyEffect, transform.position, Quaternion.identity);
                }

                OnOrbDestroyed?.Invoke();
                gameObject.SetActive(false);
            }
        }

        public void Respawn()
        {
            health = 1;
            isHittable = true;
            gameObject.SetActive(true);
        }
    }
}
