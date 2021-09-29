using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHead : MonoBehaviour
{

    public bool isKnockedDown = false;
    [SerializeField] int recoilForce = 65;
    private BossInteractionTheGolem mainInteractionScript;

    private AttackControllerTheGolem attackControllerTheGolem;

    void Start()
    {
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
        attackControllerTheGolem = GetComponentInParent<AttackControllerTheGolem>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.otherCollider.tag != "GolemHead")
        {
            return;
        }
        if (attackControllerTheGolem.finalStage)
        {
            if (col.collider.tag == "Sword")
            {
                golemFinalStageRecoil(col.gameObject);
                attackControllerTheGolem.finalStageHeadStrike();
            }
            return;
        }

        if (attackControllerTheGolem.firstStage == true)
        {
            if (col.collider.tag == "Sword" && mainInteractionScript.canTakeDamage == true)
            {
                golemHeadRecoil(col.gameObject);
                mainInteractionScript.firstStageGolemHeadHit();
            }

            if (col.gameObject.tag == "Player" && !attackControllerTheGolem.returningToOriginalPosition && col.otherCollider.tag == "Golem")
            {
                attackControllerTheGolem.startReturning();
            }
        }
        else
        {
            if (col.collider.tag == "Sword")
            {
                golemDownThrustHeadRecoil(col.gameObject);
                attackControllerTheGolem.getUp();
            }

        }

    }
    void golemHeadRecoil(GameObject collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

        rb.AddForce(Vector2.left * recoilForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * (recoilForce / 2), ForceMode2D.Impulse);
    }

    void golemDownThrustHeadRecoil(GameObject collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

        rb.AddForce(Vector2.left * recoilForce * 4, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * recoilForce * 3, ForceMode2D.Impulse);
    }

    void golemFinalStageRecoil(GameObject collider)
    {

        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

        rb.AddForce(Vector2.left * (recoilForce * 3.5f), ForceMode2D.Impulse);
        rb.AddForce(Vector2.down * (recoilForce * 2), ForceMode2D.Impulse);
    }
}
