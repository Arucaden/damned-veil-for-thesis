using System.Collections;
using System.Collections.Generic;
using ProjectLightsOut.DevUtils;
using UnityEngine;

public class EnemyShielder : EnemyHealer
{
    protected override IEnumerator Buffing()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            chantEffectAnimator.SetTrigger("Buff");

            EventManager.Broadcast(new OnBossBuff(BuffType.Shield));
        }
    }
}
