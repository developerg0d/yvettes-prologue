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

    public bool finalStage;

    public GameObject lazerBall;

    private int finalStageCounter;
    void Start()
    {
        AssignVariables();
        AssignCoroutines();
        startBattle();
    }

    void AssignVariables()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
        rightHandRb = rightHand.GetComponent<Rigidbody2D>();
    }

    void AssignCoroutines()
    {
        // raiseHandCoroutine = raiseHand();
        // spinHandCoroutine = spinHand();
        // returnToOriginalPositionCoroutine = returnToOriginalPosition();
    }

    public void startBattle()
    {
        initialHandPosition = leftHand.transform.position;
        lastHandPosition = initialHandPosition;
        StartCoroutine("startBattleCoroutine");
    }

    IEnumerator startBattleCoroutine()
    {
        canFollowPlayer = true;
        yield return new WaitForSeconds(2F);
        startedAttacking = true;
        finalStage = true;
        StartCoroutine("commenceLazerFire");
        //StartCoroutine("bouncingAttack");
        //    fallingOver();
        // StartCoroutine("startFistSlam");
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

    void Update()
    {
        if (raisingHand)
        {
            moveHandToInitialSlamPosition();
        }
        if (canFollowPlayer && !playerRidingHand)
        {
            moveHandToPlayer();
        }
    }

    void moveHandToPlayer()
    {
        float step = handFollowSpeed * Time.deltaTime;
        Vector3 offsetPlayerPosition = getOffsetPlayerPosition();
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(useOffset == true ? offsetPlayerPosition.x : player.transform.position.x, leftHand.transform.position.y), step);
    }

    void moveHandToInitialSlamPosition()
    {
        leftHandRb.velocity = new Vector2(0, handRaiseSpeed);
    }
    public void startReturning()
    {
        StopCoroutine("fistSlam");
        StopCoroutine("raiseHand");

        StartCoroutine("returnToOriginalPosition");
        canFollowPlayer = false;
        raisingHand = false;
        returningToOriginalPosition = true;
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
            if (leftHand.transform.position.y >= initialHandPosition.y)
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
        uxInteraction.updateGolemFistIndicatorPosition(leftHand.transform.position);
        yield return new WaitForSeconds(indicatorTimeout);
        StartCoroutine("fistSlam");
    }

    private IEnumerator fistSlam()
    {
        raisingHand = false;
        canFollowPlayer = false;
        slamHand();
        yield return new WaitForSeconds(5f);
        disableHandHolds();
        StartCoroutine("raiseHand");
    }

    void slamHand()
    {
        leftHandRb.AddForce(Vector2.down * slammingForce, ForceMode2D.Impulse);
        enableHandHolds();
    }
    void disableHandHolds()
    {
        GameObject[] ladders = GameObject.FindGameObjectsWithTag("Ladder");
        foreach (var ladder in ladders)
        {
            ladder.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
    void enableHandHolds()
    {
        GameObject[] ladders = GameObject.FindGameObjectsWithTag("Ladder");
        foreach (var ladder in ladders)
        {
            ladder.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    void FixedUpdate()
    {
        if (jumping)
        {
            jumping = false;
            rb.AddForce(Vector2.up * golemJumpUpForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator bouncingAttack()
    {
        while (enabled)
        {
            jumping = true;
            shockWaveSpawned = false;
            yield return new WaitForSeconds(7f);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground" && startedAttacking && !shockWaveSpawned && !hasFallenOver)
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
        StopCoroutine("bouncingAttack");
        StartCoroutine("fallOverBackwards");
    }


    IEnumerator fallOverBackwards()
    {
        while (enabled)
        {
            Vector3 direction = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -90);
            Quaternion targetRotation = Quaternion.Euler(direction);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 1);
            if (Mathf.RoundToInt(transform.eulerAngles.z) == 270)
            {
                head.SetActive(true);
                head.GetComponent<InteractionGolemHead>().isKnockedDown = true;
                StopCoroutine("fallOver");
            }
            yield return null;
        }
    }

    public void getUp()
    {
        StartCoroutine("getUpCoroutine");
    }

    IEnumerator getUpCoroutine()
    {
        yield return new WaitForSeconds(2f);
        while (enabled)
        {
            Vector3 direction = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            Quaternion targetRotation = Quaternion.Euler(direction);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 0.8f);
            if (Mathf.RoundToInt(transform.eulerAngles.z) == 00)
            {
                // head.GetComponent<InteractionGolemHead>().isKnockedDown = true;
                yield return new WaitForSeconds(2f);
                // StartCoroutine("bouncingAttack");
                golemHead2.SetActive(true);
                StartCoroutine("fallOverForwards");
                StopCoroutine("getUpCoroutine");
            }
            yield return null;
        }
    }

    IEnumerator fallOverForwards()
    {
        while (enabled)
        {
            Vector3 direction = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 90);
            Quaternion targetRotation = Quaternion.Euler(direction);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 1);
            if (Mathf.RoundToInt(transform.eulerAngles.z) == 90)
            {
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
        if (finalStageCounter == 0)
        {
            finalStageCounter++;
            StartCoroutine("commenceLazerFire");
            return;
        }
        StartCoroutine("finalPlayerSpecialAttack");
    }

    IEnumerator finalPlayerSpecialAttack()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("Player has won");
    }
}
