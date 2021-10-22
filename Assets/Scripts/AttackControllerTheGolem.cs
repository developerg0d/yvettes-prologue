using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackControllerTheGolem : MonoBehaviour
{

    [SerializeField]
    private float golemJumpUpForce;
    [SerializeField]
    private float golemJumpForwardsForce;

    [SerializeField]
    private float indicatorTimeout = 0.5F;

    [SerializeField]
    private GameObject leftHand;
    [SerializeField]
    private GameObject rightHand;

    [SerializeField]
    private float slammingForce;

    [SerializeField]
    private float handFollowSpeed;
    [SerializeField]
    private float handRaiseSpeed;

    private Rigidbody2D leftHandRb;
    private Rigidbody2D rightHandRb;

    private Vector3 initialHandPosition;

    private bool hasFallenOver;
    private bool raisingHand;

    private GameObject player;

    public bool playerRidingHand = false;

    public UxInteraction uxInteraction;

    public Vector3 offset;

    public bool jumping;

    bool startedAttacking;

    private float spinTimer = 0f;

    [SerializeField]
    private float shockWaveForce;

    public Transform shockWaveSpawnLocation;

    public GameObject shockWave;

    private bool shockWaveSpawned;
    private Rigidbody2D rb;

    public GameObject lazerCannon;

    public bool firstStage;
    public bool secondStage;
    public bool finalStage;

    public GameObject lazerBall;

    private int vb;

    public bool fistCanAttack;

    private bool isBouncing = false;

    private BossInteractionTheGolem bossInteractionTheGolem;

    private Animator golemAnimator;
    float xOffSet;
    void Start()
    {
        assignVariables();
        startBattle();
    }

    void assignVariables()
    {
        golemAnimator = this.GetComponent<Animator>();
        bossInteractionTheGolem = GetComponent<BossInteractionTheGolem>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
        rightHandRb = rightHand.GetComponent<Rigidbody2D>();
    }

    public void startBattle()
    {
        initialHandPosition = leftHand.transform.position;
        StartCoroutine("startBattleCoroutine");
    }

    IEnumerator startBattleCoroutine()
    {
        yield return new WaitForSeconds(2F);
        startedAttacking = true;
        firstStage = true;
        StartCoroutine("startFistSlamCoroutine");
    }

    IEnumerator startFistSlamCoroutine()
    {
        Debug.Log("Slam Fist");

        uxInteraction.updateGolemFistIndicatorPosition(player.transform.position);
        yield return new WaitForSeconds(indicatorTimeout);
        StartCoroutine("fistSlamCoroutine");
    }

    private IEnumerator fistSlamCoroutine()
    {
        slamFist();
        yield return new WaitForSeconds(5f);
        StartCoroutine("raiseHandCoroutine");
    }

    void slamFist()
    {
        Debug.Log("Slamming Fist");
        Vector3 test = leftHand.transform.right = player.transform.position - leftHand.transform.position;
        leftHand.transform.eulerAngles = Vector3.zero;
        leftHandRb.AddForce(test * slammingForce, ForceMode2D.Impulse);
    }
    IEnumerator raiseHandCoroutine()
    {
        Debug.Log("Raising Hand");

        while (enabled)
        {
            float step = handRaiseSpeed * Time.fixedDeltaTime;
            leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(initialHandPosition.x, initialHandPosition.y), step);

            if (leftHand.transform.position.y >= initialHandPosition.y && leftHand.transform.position.x >= initialHandPosition.x)
            {
                Debug.Log("Raised Hand");

                yield return new WaitForSeconds(2f);
                if (playerRidingHand)
                {
                    spinTimer = 0;
                    StartCoroutine("spinHandCoroutine");
                }
                else
                {
                    StartCoroutine("startFistSlamCoroutine");
                }
                
                StopCoroutine("raiseHandCoroutine");
            }
            yield return null;
        }
    }

    IEnumerator spinHandCoroutine()
    {
        while (enabled)
        {
            spinTimer += Time.deltaTime;
            leftHand.transform.Rotate(0, 0, 250 * Time.deltaTime);
            if (spinTimer >= 2)
            {
                leftHand.transform.eulerAngles = new Vector3(0, 0, 0);
                StopCoroutine("spinHand");
            }
            yield return null;
        }
    }

    public void onGolemFirstStage()
    {
        StopCoroutine("startFistSlamCoroutine");
        StopCoroutine("raiseHand");
        StopCoroutine("fistSlam");
        StartCoroutine("waitForGolemToShakeOff");
    }
    IEnumerator waitForGolemToShakeOff()
    {
        yield return new WaitForSeconds(15f);
        Debug.Log("Start Shaking");
        player.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 150);
    }


    public void startSecondStage()
    {
        StopCoroutine("startFistSlamCoroutine");
        StopCoroutine("raiseHand");
        StopCoroutine("fistSlamCoroutine");
        StartCoroutine("secondStageCoroutine");
    }

    IEnumerator secondStageCoroutine()
    {
        firstStage = false;
        yield return new WaitForSeconds(2f);
        startedAttacking = true;
        secondStage = true;
        Debug.Log("bouncing");
        bouncingAttack();
    }

    void bouncingAttack()
    {
        StartCoroutine("bouncingAttackCoroutine");
    }
    IEnumerator bouncingAttackCoroutine()
    {
        golemAnimator.SetBool("prepareToJump", true);
        float randomWaitingTime = Random.Range(5, 7.5f);
        while (enabled)
        {
            yield return new WaitForSeconds(randomWaitingTime);
            golemAnimator.SetTrigger("jump");
            isBouncing = true;
            rb.AddForce(Vector2.up * golemJumpUpForce, ForceMode2D.Impulse);
            shockWaveSpawned = false;
            randomWaitingTime = Random.Range(5, 7.5f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground" && isBouncing && !shockWaveSpawned)
        {
            shockWaveSpawned = true;
            spawnShockWaves();
        }
    }

    void spawnShockWaves()
    {
        GameObject shock = Instantiate(shockWave) as GameObject;
        shock.transform.position = shockWaveSpawnLocation.position;
        shock.GetComponent<Rigidbody2D>().AddForce(Vector2.left * shockWaveForce, ForceMode2D.Impulse);
    }

    public void beenParried()
    {
        isBouncing = false;
        golemAnimator.SetBool("prepareToJump", false);
        rb.bodyType = RigidbodyType2D.Static;
        golemAnimator.SetBool("fallenOver", true);
        startedAttacking = false;
        StopCoroutine("bouncingAttackCoroutine");
    }

    public void getUp()
    {
        golemAnimator.SetBool("fallenOver", false);
        golemAnimator.SetTrigger("getUp");
        Debug.Log("get up");
        StartCoroutine("getUpCoroutine");
    }

    IEnumerator getUpCoroutine()
    {
        yield return new WaitForSeconds(21f);
        golemAnimator.SetTrigger("fallForwards");
        yield return new WaitForSeconds(10f);
        // bouncingAttack();
        commenceFinalStage();
    }

    private void commenceFinalStage()
    {
        rb.bodyType = RigidbodyType2D.Static;
        finalStage = true;
        secondStage = false;
        StartCoroutine("commenceLazerFire");
        StopCoroutine("fallOverForwards");
    }

    public void lazerBallParry()
    {
        StopCoroutine("lazerEyesTrackPlayer");
    }

    IEnumerator commenceLazerFire()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine("lazerEyesTrackPlayer");
    }

    IEnumerator lazerEyesTrackPlayer()
    {
        while (enabled)
        {
            lazerCannon.transform.right = player.transform.position - lazerCannon.transform.position;
            fireLazerBall();
            yield return new WaitForSeconds(2f);
        }
    }

    void fireLazerBall()
    {
        Debug.Log("Fire");
        GameObject lazerBallInstance = Instantiate(lazerBall, lazerCannon.transform);
        lazerBallInstance.transform.position = lazerCannon.transform.position;
        lazerBallInstance.transform.rotation = lazerCannon.transform.rotation;
    }

    public void finalStageHeadStrike()
    {
        // if (bossInteractionTheGolem.finalStageCounter == 0)
        // {
        //     bossInteractionTheGolem.finalStageCounter++;
        //     StartCoroutine("commenceLazerFire");
        //     return;
        // }
        StartCoroutine("finalPlayerSpecialAttack");
    }

    IEnumerator finalPlayerSpecialAttack()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("Player has won");
    }
}
