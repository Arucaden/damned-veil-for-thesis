using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class VoidProjectile : Projectile
    {
        public static bool CurrentlyFiring { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            CurrentlyFiring = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            CurrentlyFiring = false;
        }

        private void OnDisable()
        {
            CurrentlyFiring = false;
        }
    }
}
