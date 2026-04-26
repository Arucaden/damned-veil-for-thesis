using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class OneWayPortal : PortalBase
    {

        [Header("Trickshot Movement Settings")]
        [Tooltip("If true, the portal requires at least 1 ricochet to work. If bullet approaches directly (0 ricochets), it dodges.")]
        [SerializeField] private bool requiresRicochet = true;
        [SerializeField] private float dodgeRadius = 3f;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 10f;
        [SerializeField] private bool loop = true;

        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private Transform currentTarget;
        private LayerMask projectileLayerMask;

        private void Start()
        {
            projectileLayerMask = LayerMask.GetMask("Projectile");
            if (projectileLayerMask.value == 0) projectileLayerMask = Physics2D.AllLayers;
        }

        private void Update()
        {
            if (requiresRicochet && !isMoving && waypoints != null && waypoints.Length > 0)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, dodgeRadius, projectileLayerMask);
                foreach (var col in colliders)
                {
                    Projectile proj = col.GetComponent<Projectile>();
                    if (proj != null && proj.RicochetCount == 0)
                    {
                        MoveToNextWaypoint();
                        break;
                    }
                }
            }

            if (isMoving && currentTarget != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
                if (Vector3.Distance(transform.position, currentTarget.position) < 0.01f)
                {
                    isMoving = false;
                }
            }
        }

        private void MoveToNextWaypoint()
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loop) currentWaypointIndex = 0;
                else currentWaypointIndex = waypoints.Length - 1;
            }
            currentTarget = waypoints[currentWaypointIndex];
            isMoving = true;
            
        }

        protected override bool CanEnter(Projectile projectile, Vector2 hitPoint, Vector2 hitNormal)
        {
            if (requiresRicochet && projectile.RicochetCount == 0)
            {
                return false; 
            }

            return true;
        }

        public override Vector2 GetExitPosition()
        {
            return transform.position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (requiresRicochet)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, dodgeRadius);
            }
        }
#endif
    }
}
