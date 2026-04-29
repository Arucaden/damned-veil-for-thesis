using UnityEngine;

namespace ProjectLightsOut.Gameplay
{
    public class PortalVisuals : MonoBehaviour
    {
        [Header("Spin Settings")]
        [Tooltip("The Transform holding the portal sprite. Leave empty to spin this object.")]
        [SerializeField] private Transform spriteTransform;
        [SerializeField] private float spinSpeed = 180f;

        private void Awake()
        {
            if (spriteTransform == null)
            {
                spriteTransform = transform;
            }
        }

        private void Update()
        {
            // Spin the sprite along the Z axis
            if (spriteTransform != null)
            {
                spriteTransform.Rotate(0, 0, spinSpeed * Time.deltaTime);
            }
        }
    }
}
