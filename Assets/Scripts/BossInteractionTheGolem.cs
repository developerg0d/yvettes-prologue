using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInteractionTheGolem : MonoBehaviour
{
    public bool canTakeDamage = true;
    public UxInteraction uxInteraction;

    private AttackControllerTheGolem attackControllerTheGolem;

    private BossStats bossStats;

    public int firstStageCounter;
    public int finalStageCounter;

    public GameObject leftHand;
    public GameObject rightHand;

    public BoxCollider2D leftFootCollider;
    public BoxCollider2D rightFootCollider;

    public Camera MediumCamera;
    public Camera CloseCamera;
    public Camera WideCamera;
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
    public void firstStageGolemHeadHit()
    {
        bossStats.currentHp -= 200;
        uxInteraction.updateBossHpBar(bossStats.currentHp);
        firstStageCounter++;
        if (firstStageCounter == 1)
        {
            leftHand.SetActive(false);
            rightHand.SetActive(false);
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.None;
            rb.gravityScale = 1.0f;
            attackControllerTheGolem.startSecondStage();
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {

        if (attackControllerTheGolem.firstStage)
        {
            if (col.gameObject.tag == "Player" && col.otherCollider.tag == "Golem")
            {
                attackControllerTheGolem.onGolemFirstStage();
            }
        }

        if (col.gameObject.tag == "LazerBall" && col.gameObject.GetComponent<lazerBall>().beenParried)
        {
            Debug.Log("parried");
            attackControllerTheGolem.lazerBallParry();
            attackControllerTheGolem.finalStageHeadStrike();
            Destroy(col.gameObject);
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
