using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;

public class BossInteractionTheGolem : MonoBehaviour
{
    public bool onFist = false;
    public bool canTakeDamage = true;
    public UxInteraction uxInteraction;

    private GolemAttackController golemAttackController;
    private BossStateManager bossStateManager;
    private BossStats bossStats;

    public int firstStageCounter;
    public int secondStageCounter;
    public int finalStageCounter;

    public GameObject leftHand;
    public GameObject rightHand;

    public BoxCollider2D leftFootCollider;
    public BoxCollider2D rightFootCollider;

    [SerializeField] private float closeCameraSize;
    [SerializeField] private float mediumCameraSize;
    [SerializeField] private float longCameraSize;

    public bool playerOnGolem = false;
    private bool hasBeenParried;
    public Cinemachine.CinemachineVirtualCamera mainCamera;

    void Start()
    {
        bossStateManager = GetComponent<BossStateManager>();
        golemAttackController = GetComponent<GolemAttackController>();
        bossStats = GetComponent<BossStats>();
    }

    public void golemHandHit()
    {
        bossStats.currentHp -= 50;
        uxInteraction.updateBossHpBar(bossStats.currentHp);
    }

    public void firstStageHeadHit()
    {
        bossStats.currentHp -= 200;
        uxInteraction.updateBossHpBar(bossStats.currentHp);
        firstStageCounter++;
        if (firstStageCounter == 2)
        {
            bossStateManager.NextBossStage();
        }
    }

    public void secondStageHeadHit()
    {
        bossStats.currentHp -= 200;
        uxInteraction.updateBossHpBar(bossStats.currentHp);
        secondStageCounter++;
        golemAttackController.getUp();
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player" && playerOnGolem &&
            bossStateManager.currentStage == (int) GolemAttackController.BossStages.FirstStage)
        {
            StartCoroutine("playerExitDelay");
        }
    }

    IEnumerator playerExitDelay()
    {
        yield return new WaitForSeconds(6f);
        Debug.Log("player exited");
        golemAttackController.startFirstStage();
        playerOnGolem = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Shockwave")
        {
            if (col.gameObject.GetComponentInParent<ShockwaveInteraction>().beenParried)
            {
                Destroy(col.GetComponentInChildren<BoxCollider2D>());
                StartCoroutine(nameof(shockwaveDispel), col.gameObject);
                golemAttackController.beenParried();
            }
        }
    }

    IEnumerator shockwaveDispel(GameObject shockwave)
    {
        yield return new WaitForSeconds(1f);
        Destroy(shockwave);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (bossStateManager.currentStage == (int) GolemAttackController.BossStages.FirstStage)
        {
            if (col.gameObject.tag == "Player" && playerOnGolem)
            {
                StopCoroutine("playerExitDelay");
            }

            if (col.gameObject.tag == "Player" && col.otherCollider.tag == "Golem")
            {
                if (!playerOnGolem)
                {
                    playerOnGolem = true;
                    golemAttackController.playerOnTopGolemFirstStage();
                }
            }
        }
    }
}