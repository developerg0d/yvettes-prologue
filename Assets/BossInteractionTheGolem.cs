using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInteractionTheGolem : MonoBehaviour
{
    public bool canTakeDamage = true;
    public UxInteraction uxInteraction;

    private BossStats bossStats;
    void Start()
    {
        bossStats = GetComponent<BossStats>();
    }
    public void golemHandHit()
    {
        bossStats.currentHp -= 50;
        uxInteraction.updateBossHpBar(bossStats.currentHp);
    }
    public void golemHeadHit()
    {
        bossStats.currentHp -= 200;
        uxInteraction.updateBossHpBar(bossStats.currentHp);
    }
}
