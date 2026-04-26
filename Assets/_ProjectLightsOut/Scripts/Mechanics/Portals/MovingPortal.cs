using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class MovingPortal : PortalBase
    {
        [Header("Movement Settings")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool loop = true;

        private int currentWaypointIndex = 0;

        private void Update()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.01f)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    if (loop) currentWaypointIndex = 0;
                    else currentWaypointIndex = waypoints.Length - 1;
                }
            }
        }
    }
}
