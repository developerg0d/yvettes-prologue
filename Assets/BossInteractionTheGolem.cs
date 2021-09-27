using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInteractionTheGolem : MonoBehaviour
{
    public bool canTakeDamage = true;
    public UxInteraction uxInteraction;

    private AttackControllerTheGolem attackControllerTheGolem;

    private BossStats bossStats;
    void Start()
    {
        attackControllerTheGolem = GetComponent<AttackControllerTheGolem>();
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
    void OnCollisionExit2D(Collision2D col)
    {

        if (col.gameObject.tag == "Player" && col.collider.tag == "ScalingWall")
        {
            // attackControllerTheGolem.canFollowPlayer = true;
        }

    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "LazerBall" && col.GetComponent<lazerBall>().beenParried)
        {
            Debug.Log("parried");
            attackControllerTheGolem.lazerBallParry();
            Destroy(col.gameObject);
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player" && col.otherCollider.tag == "ScalingWall" && !attackControllerTheGolem.returningToOriginalPosition)
        {
            attackControllerTheGolem.startReturning();
        }

        if (col.gameObject.tag == "Shockwave")
        {
            if (col.gameObject.GetComponent<shockwaveInteraction>().beenParried)
            {
                Destroy(col.gameObject);
                attackControllerTheGolem.beenParried();
            }
        }

    }
}
