using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Vector2 direction;
        private int ricochetCount;
        [SerializeField] private int maxRicochetCount = 3;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            rb.velocity = direction;
        }

        public void SetDirection(Vector2 direction)
        {
            this.direction = direction;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Ricochet"))
            {
                if (ricochetCount < maxRicochetCount)
                {
                    ricochetCount++;
                    direction = Vector2.Reflect(direction, collision.GetContact(0).normal);
                    transform.up = direction;
                }

                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}