using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInteractionTheGolem : MonoBehaviour
{
    public bool canTakeDamage = true;

    private BossStats bossStats;
    void Start()
    {
        bossStats = GetComponent<BossStats>();
    }
    public void golemHandHit()
    {
        bossStats.currentHp -= 1;
        Debug.Log("ow");
    }
}
