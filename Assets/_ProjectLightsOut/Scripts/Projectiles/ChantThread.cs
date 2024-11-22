using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using UnityEngine;

public class ChantThread : BossBuffThread
{
    protected override void Arrived()
    {
        EventManager.Broadcast(new OnEnemyChant());
        Destroy(gameObject);
    }
}
