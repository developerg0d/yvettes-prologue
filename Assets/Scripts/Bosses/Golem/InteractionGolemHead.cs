using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHead : MonoBehaviour
{

    public bool isKnockedDown = false;
    [SerializeField] int recoilForce = 65;
    private BossInteractionTheGolem mainInteractionScript;

    private GolemAttackController attackControllerTheGolem;

    private bool beenHitDelay;

    void Start()
    {
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
        attackControllerTheGolem = GetComponentInParent<GolemAttackController>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.gameObject.tag == "LazerBall" && col.gameObject.GetComponent<lazerBall>().beenParried)
        {
            Debug.Log("parried");
            attackControllerTheGolem.lazerBallParry();
            attackControllerTheGolem.finalStageHeadStrike();
            Destroy(col.gameObject);
        }

        if (beenHitDelay)
        {
            return;
        }

        if (attackControllerTheGolem.finalStage)
        {
            if (col.gameObject.tag == "Sword")
            {
                StartCoroutine("hitDelay");
                golemFinalStageRecoil(col.gameObject);
                // attackControllerTheGolem.finalStageHeadStrike();
            }
            return;
        }

        if (attackControllerTheGolem.firstStage == true)
        {
            if (col.gameObject.tag == "Sword" && mainInteractionScript.canTakeDamage == true)
            {
                StartCoroutine("hitDelay");
                golemHeadRecoil(col.gameObject);
                mainInteractionScript.firstStageGolemHeadHit();
                return;
            }
        }
        if (attackControllerTheGolem.secondStage == true)
        {
            if (col.gameObject.tag == "Sword")
            {
                golemDownThrustHeadRecoil(col.gameObject);
                attackControllerTheGolem.getUp();
            }
        }
    }

    IEnumerator hitDelay()
    {
        yield return new WaitForSeconds(5f);
        beenHitDelay = false;
    }
    void golemHeadRecoil(GameObject collider)
    {
        StartCoroutine("headRecoil", collider);
    }

    IEnumerator headRecoil(GameObject collider)
    {
        Rigidbody2D rb = collider.GetComponentInParent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * (recoilForce * 0.75f), ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.075f);
        rb.AddForce(Vector2.left * recoilForce, ForceMode2D.Impulse);
    }

    void golemDownThrustHeadRecoil(GameObject collider)
    {
        Rigidbody2D rb = collider.GetComponentInParent<Rigidbody2D>();
        rb.AddForce(Vector2.left * recoilForce * 3, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * recoilForce * 2, ForceMode2D.Impulse);
    }

    void golemFinalStageRecoil(GameObject collider)
    {

        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

        rb.AddForce(Vector2.left * (recoilForce * 3.5f), ForceMode2D.Impulse);
        rb.AddForce(Vector2.down * (recoilForce * 2), ForceMode2D.Impulse);
    }
}
