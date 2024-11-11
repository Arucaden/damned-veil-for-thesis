using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class PlayerSpriteHandler : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerShoot playerShoot;
        [SerializeField] private PlayerMove playerMove;
        [SerializeField] private Animator castEffectAnimator;

        private void OnEnable()
        {
            playerShoot.OnShoot += OnShoot;
            playerMove.OnPlayerMoving += OnPlayerMoving;
        }

        private void OnDisable()
        {
            playerShoot.OnShoot -= OnShoot;
            playerMove.OnPlayerMoving -= OnPlayerMoving;
        }

        private void Update()
        {
            FlipSprite();
        }

        private void OnShoot()
        {
            animator.SetTrigger("Shoot");
            castEffectAnimator.SetTrigger("Cast");
        }

        private void OnPlayerMoving(bool isMoving)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        private void FlipSprite()
        {
            if (playerShoot.Direction.x > 0)
            {
                spriteRenderer.flipX = false;
            }
            
            else if (playerShoot.Direction.x < 0)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}