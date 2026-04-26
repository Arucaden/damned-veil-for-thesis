using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public abstract class PortalBase : MonoBehaviour
    {
        [Tooltip("The portal this one connects to.")]
        [SerializeField] protected PortalBase linkedPortal;
        
        [Tooltip("Where the bullet should exit. If null, uses the center of the linked portal.")]
        [SerializeField] protected Transform exitPoint;

        public PortalBase LinkedPortal => linkedPortal;

        public virtual bool TryEnterPortal(Projectile projectile, Vector2 hitPoint, Vector2 hitNormal, out Vector2 exitPos, out Vector2 exitDir)
        {
            exitPos = Vector2.zero;
            exitDir = Vector2.zero;

            if (linkedPortal == null) 
            {
                Debug.LogWarning($"[PortalBase] {name} has no linked portal! Bullet rejected.");
                return false;
            }

            if (!CanEnter(projectile, hitPoint, hitNormal))
            {
                return false;
            }

            // Calculate relative exit direction
            Transform exitTransform = linkedPortal.GetExitTransform();
            Vector2 localDir = transform.InverseTransformDirection(projectile.Direction);
            
            // Flip the Y velocity so it comes OUT of the exit portal instead of going IN
            Vector2 exitLocalDir = new Vector2(localDir.x, -localDir.y);
            exitDir = exitTransform.TransformDirection(exitLocalDir);

            exitPos = linkedPortal.GetExitPosition();
            
            OnPortalEntered(projectile);
            linkedPortal.OnPortalExited(projectile);

            return true;
        }


        protected virtual bool CanEnter(Projectile projectile, Vector2 hitPoint, Vector2 hitNormal)
        {
            return true;
        }

        public virtual Vector2 GetExitPosition()
        {
            return GetExitTransform().position;
        }

        public virtual Transform GetExitTransform()
        {
            return exitPoint != null ? exitPoint : transform;
        }

        protected virtual void OnPortalEntered(Projectile projectile)
        {
            // Hook for VFX/SFX
        }

        protected virtual void OnPortalExited(Projectile projectile)
        {
            // Hook for VFX/SFX
        }
    }
}
