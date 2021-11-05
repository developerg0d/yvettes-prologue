using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class InteractionGolemHead : MonoBehaviour
{
    public bool isKnockedDown = false;
    [SerializeField] int recoilForce = 65;
    private BossInteractionTheGolem mainInteractionScript;

    private GolemAttackController attackControllerTheGolem;

    public CameraShake cameraShake;
    public BossStateManager bossStateManager;

    private bool beenHitDelay;

    void Start()
    {
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
        attackControllerTheGolem = GetComponentInParent<GolemAttackController>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (beenHitDelay)
        {
            return;
        }

        if (col.gameObject.CompareTag("LazerBall") && col.gameObject.GetComponent<lazerBall>().beenParried)
        {
            lazerBall lazerBallScript = col.gameObject.GetComponent<lazerBall>();
            StartCoroutine(nameof(hitDelay));
            Debug.Log("parried");
            if (lazerBallScript.bounceCounter == 0)
            {
                cameraShake.shakeCamera(0.1f, 0.1f);
                col.gameObject.transform.localScale = new Vector3(2, 2, 2);
                lazerBallScript.bounceCounter++;
                col.gameObject.GetComponent<lazerBall>().lazerBallSpeed =
                    -col.gameObject.GetComponent<lazerBall>().lazerBallSpeed * 1.5f;
                return;
            }

            cameraShake.shakeCamera(0.7f, 0.4f);
            attackControllerTheGolem.lazerBallParry();
            Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("Sword"))
        {
            swordHitGolem(col.gameObject);
        }
    }

    void swordHitGolem(GameObject player)
    {
        StartCoroutine(nameof(hitDelay));
        switch (bossStateManager.currentStage)
        {
            case (int) GolemAttackController.BossStages.FirstStage:
                golemHeadRecoil(player);
                mainInteractionScript.firstStageHeadHit();
                return;
            case (int) GolemAttackController.BossStages.SecondStage:
                golemDownThrustHeadRecoil(player);
                mainInteractionScript.secondStageHeadHit();
                return;
            case (int) GolemAttackController.BossStages.ThirdStage:
                golemFinalStageRecoil(player);
                attackControllerTheGolem.finalStageHeadStrike();
                return;
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
        Rigidbody2D rb = collider.GetComponentInParent<Rigidbody2D>();

        rb.AddForce(Vector2.left * (recoilForce * 3.5f), ForceMode2D.Impulse);
        rb.AddForce(Vector2.down * (recoilForce * 2), ForceMode2D.Impulse);
    }
}