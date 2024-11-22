using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D col2D;
    private bool isShieldActive = false;
    private float shieldCharge = 1.5f;
    private float currentShieldCharge = 0f;
    private float lastShieldTime = 0f;
    private Coroutine shieldCoroutine;

    private void Awake()
    {
        spriteRenderer.enabled = false;
        animator.enabled = false;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
        col2D.enabled = false;
    }

    private void Update()
    {
        if (lastShieldTime > 0)
        {
            lastShieldTime -= Time.deltaTime;
            if (lastShieldTime <= 0)
            {
                DeactivateShield();
            }
        }

        if (lastShieldTime > 0)
        {
            if (currentShieldCharge < shieldCharge)
            {
                currentShieldCharge += Time.deltaTime;
            }
            
            else
            {
                ActivateShield();
            }
        }
    }

    public void ChargeShield()
    {
        lastShieldTime = 0.3f;
    }

    private void ActivateShield()
    {
        if (isShieldActive) return;
        isShieldActive = true;
        spriteRenderer.enabled = true;
        animator.enabled = true;
        col2D.enabled = true;

        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }
        shieldCoroutine = StartCoroutine(ShieldUpAnimation());
    }

    public void DeactivateShield()
    {
        if (!isShieldActive) return;
        isShieldActive = false;
        col2D.enabled = false;
        isShieldActive = false;
        currentShieldCharge = 0f;
        
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }

        shieldCoroutine = StartCoroutine(ShieldDownAnimation());
    }

    private IEnumerator ShieldUpAnimation()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            yield return null;
        }
    }

    private IEnumerator ShieldDownAnimation()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            yield return null;
        }

        spriteRenderer.enabled = false;
        animator.enabled = false;
    }
}
