using System;
using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using UnityEngine;

public enum BuffType
{
    Health,
    Shield
}

public class OnBossBuff : GameEvent
{
    public BuffType buffType;

    public OnBossBuff(BuffType buffType)
    {
        this.buffType = buffType;
    }
}

public class BossBuffThread : MonoBehaviour
{
    private Transform targetTransform;
    [SerializeField] private BuffType buffType;
    public Action OnThreadDestroyed;

    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    protected virtual void Update()
    {
        transform.position = Vector2.Lerp(transform.position, targetTransform.position, Time.deltaTime * 1);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, targetTransform.position - transform.position);

        if (Vector2.Distance(transform.position, targetTransform.position) < 0.3f)
        {
            Arrived();
        }
    }

    protected virtual void Arrived()
    {
        EventManager.Broadcast(new OnBossBuff(buffType));
        OnThreadDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
