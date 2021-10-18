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
    private Vector3 lastHandPosition;

    private bool slammingHand;

    private bool hasFallenOver;
    private bool raisingHand;

    [SerializeField]
    private GameObject head;
    public bool canFollowPlayer = true;

    private GameObject player;

    public bool playerRidingHand = false;

    public UxInteraction uxInteraction;

    public Vector3 offset;
    public bool returningToOriginalPosition;

    public bool useOffset;

    public bool jumping;

    bool startedAttacking;

    private float spinTimer = 0f;

    [SerializeField]
    private float shockWaveForce;

    public Transform shockWaveSpawnLocation;

    public GameObject shockWave;

    private bool shockWaveSpawned;

    public GameObject golemHead2;
    private Rigidbody2D rb;

    public GameObject lazerCannon;

    public bool firstStage;
    public bool secondStage;
    public bool finalStage;

    public GameObject lazerBall;

    private int finalStageCounter;

    public bool fistCanAttack;

    private bool isBouncing = false;

    private BossInteractionTheGolem bossInteractionTheGolem;

    private Animator golemAnimator;
    float xOffSet;
    void Start()
    {
        assignVariables();
        StartCoroutine("secondStageCoroutine");
        // startBattle();
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
        fistCanAttack = true;
        canFollowPlayer = true;
        yield return new WaitForSeconds(2F);
        startedAttacking = true;
        firstStage = true;
        StartCoroutine("startFistSlam");
    }

    void fallingOver()
    {
        hasFallenOver = true;
        enableHandHolds();
        StartCoroutine("fallover");
    }

    Vector3 getOffsetPlayerPosition()
    {
        Vector3 offSetPlayerPosition = player.transform.position;
        offSetPlayerPosition.x += offset.x;
        offSetPlayerPosition.y += offset.y;
        return offSetPlayerPosition;
    }

    void FixedUpdate()
    {

        if (firstStage)
        {
            if (raisingHand)
            {
                moveHandToInitialSlamPosition();
            }
        }
    }

    void moveHandToPlayer()
    {
        bool tooCloseToBoss = leftHand.GetComponent<InteractionGolemHand>().tooCloseToBoss;
        float step = handFollowSpeed * Time.deltaTime;
        if (tooCloseToBoss)
        {
            xOffSet += 0.1f;
        }
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(player.transform.position.x - xOffSet, leftHand.transform.position.y), step);
    }

    void moveHandToInitialSlamPosition()
    {
        float step = handRaiseSpeed * Time.fixedDeltaTime;
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(initialHandPosition.x, initialHandPosition.y), step);
    }

    public void onGolemFirstStage()
    {
        StopCoroutine("startFistSlam");
        StopCoroutine("raiseHand");
        StopCoroutine("fistSlam");
        fistCanAttack = false;
        StartCoroutine("waitForGolemToShakeOff");
    }

    IEnumerator waitForGolemToShakeOff()
    {
        yield return new WaitForSeconds(15f);
        Debug.Log("Start Shaking");
        player.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 150);
    }
    IEnumerator returnToOriginalPosition()
    {
        while (enabled)
        {
            float step = handFollowSpeed * Time.deltaTime;
            leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, initialHandPosition, step);

            if (leftHand.transform.position.x == initialHandPosition.x && leftHand.transform.position.y == initialHandPosition.y)
            {
                leftHandRb.velocity = Vector2.zero;
                StopAllCoroutines();
            }
            yield return null;
        }
    }

    IEnumerator raiseHand()
    {
        while (enabled)
        {

            if (!playerRidingHand)
            {
                canFollowPlayer = true;
            }
            raisingHand = true;
            if (leftHand.transform.position.y == initialHandPosition.y && leftHand.transform.position.x == initialHandPosition.x)
            {
                raisingHand = false;
                yield return new WaitForSeconds(2f);
                if (playerRidingHand)
                {
                    spinTimer = 0;
                    StartCoroutine("spinHand");
                }
                else
                {
                    StartCoroutine("startFistSlam");
                    canFollowPlayer = true;
                }
                StopCoroutine("raiseHand");
            }
            yield return null;
        }
    }
    IEnumerator spinHand()
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

    IEnumerator startFistSlam()
    {
        uxInteraction.updateGolemFistIndicatorPosition(player.transform.position);
        yield return new WaitForSeconds(indicatorTimeout);
        StartCoroutine("fistSlam");
    }

    private IEnumerator fistSlam()
    {
        raisingHand = false;
        canFollowPlayer = false;
        slamHand();
        yield return new WaitForSeconds(5f);
        StartCoroutine("raiseHand");
    }

    void slamHand()
    {
        Vector3 test = leftHand.transform.right = player.transform.position - leftHand.transform.position;
        leftHand.transform.eulerAngles = Vector3.zero;
        leftHandRb.AddForce(test * slammingForce, ForceMode2D.Impulse);
        enableHandHolds();
    }
    public void disableHandHolds()
    {
        GameObject[] ladders = GameObject.FindGameObjectsWithTag("Ladder");
        foreach (var ladder in ladders)
        {
            ladder.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
    public void enableHandHolds()
    {
        GameObject[] ladders = GameObject.FindGameObjectsWithTag("Ladder");
        foreach (var ladder in ladders)
        {
            ladder.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    public void startSecondStage()
    {
        StopCoroutine("startFistSlam");
        StopCoroutine("raiseHand");
        StopCoroutine("fistSlam");
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
        rb.bodyType = RigidbodyType2D.Dynamic;
        yield return new WaitForSeconds(2f);
        bouncingAttack();
    }

    IEnumerator fallOverForwards()
    {
        while (enabled)
        {
            Vector3 direction = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 90);
            Quaternion targetRotation = Quaternion.Euler(direction);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 1);
            if (transform.eulerAngles.z >= 87 && transform.eulerAngles.z <= 91)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX;
                finalStage = true;
                secondStage = false;
                StartCoroutine("commenceLazerFire");
                StopCoroutine("fallOverForwards");
            }
            yield return null;
        }
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
