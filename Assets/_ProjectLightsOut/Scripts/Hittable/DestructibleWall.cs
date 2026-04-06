using UnityEngine;
using ProjectLightsOut.Gameplay;

namespace ProjectLightsOut.Hittable
{
    /// <summary>
    /// A destructible wall that lets procedural generation pass through it 
    /// (by assigning it to an excluded layer in the spawner), 
    /// but still ricochets the actual Projectile and gets destroyed upon impact.
    /// </summary>
    public class DestructibleWall : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Check if the collided object has the Projectile component
            if (collision.gameObject.GetComponent<Projectile>() != null)
            {
                // You can add VFX/SFX here right before destruction!
                // Example: Instantiate(destructionVFX, transform.position, Quaternion.identity);

                // We add a tiny 0.05s delay before destroying the object.
                // This guarantees the Projectile has time to register the collision
                // and bounce off it before the wall disappears.
                Destroy(gameObject, 0.05f);
            }
        }
    }
}
