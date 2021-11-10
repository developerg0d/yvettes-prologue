using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GolemAttackController : MonoBehaviour
{
    [Header("Golem Physic Forces")] [Space(10)] [SerializeField]
    private float golemJumpUpForce;

    [SerializeField] private float shockWaveForce;

    [SerializeField] private float slammingForce;

    [SerializeField] private float handRaiseSpeed;

    private Rigidbody2D leftHandRb;

    private Vector3 initialHandPosition;

    private GameObject player;

    public CameraShake cameraShake;

    [Space(10)] [SerializeField] private float indicatorTimeout = 0.5F;

    public GameObject leftHand;
    public GameObject leftHandClimbingHolds;
    public UxInteraction uxInteraction;

    private float spinTimer = 0f;

    public Transform shockWaveSpawnLocation;

    public GameObject shockWave;

    private bool shockWaveSpawned;
    private Rigidbody2D rb;

    public GameObject lazerCannon;

    public GameObject lightLazerBall;
    public GameObject darkLazerBall;

    public GameObject floatingEye;

    private bool isBouncing;

    public UnityEngine.U2D.PixelPerfectCamera pixelPerfectCamera;
    private BossInteractionTheGolem bossInteractionTheGolem;

    private BossStateManager bossStateManager;
    private Animator golemAnimator;
    public Collider2D[] golemFirstStageCollision;
    public Collider2D[] golemSecondStageCollision;

    public bool isFalling;

    public enum BossStages
    {
        FirstStage,
        SecondStage,
        ThirdStage,
    }

    void Start()
    {
        assignVariables();
    }

    void assignVariables()
    {
        bossStateManager = GetComponent<BossStateManager>();
        golemAnimator = GetComponent<Animator>();
        bossInteractionTheGolem = GetComponent<BossInteractionTheGolem>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
    }

    public void startFirstStage()
    {
        initialHandPosition = leftHand.transform.position;
        StartCoroutine(nameof(startFirstStageCoroutine));
    }

    IEnumerator startFirstStageCoroutine()
    {
        yield return new WaitForSeconds(2F);
        StartCoroutine(nameof(startFistSlamCoroutine));
    }

    IEnumerator startFistSlamCoroutine()
    {
        Debug.Log("Slam Fist");

        uxInteraction.updateGolemFistIndicatorPosition(leftHand.transform.position, player.transform.position);
        yield return new WaitForSeconds(indicatorTimeout);
        StartCoroutine(nameof(fistSlamCoroutine));
    }

    private IEnumerator fistSlamCoroutine()
    {
        slamFist();
        yield return new WaitForSeconds(5f);
        StartCoroutine(nameof(raiseHandCoroutine));
    }

    public void retractHandInstantly()
    {
        StopCoroutine(nameof(fistSlamCoroutine));
        StartCoroutine(nameof(raiseHandCoroutine));
    }

    void slamFist()
    {
        Debug.Log("Slamming Fist");
        leftHand.GetComponent<InteractionGolemHand>().IsSlamming = true;
        Vector3 test = leftHand.transform.right = player.transform.position - leftHand.transform.position;
        leftHand.transform.eulerAngles = Vector3.zero;
        leftHandRb.AddForce(test * slammingForce, ForceMode2D.Impulse);
    }

    IEnumerator raiseHandCoroutine()
    {
        leftHand.GetComponent<InteractionGolemHand>().groundExit();
        Debug.Log("Raising Hand");
        while (enabled)
        {
            raiseHand();

            if (leftHand.transform.position.y >= pixelPerfectCamera.RoundToPixel(initialHandPosition).y)
            {
                Debug.Log("Raised Hand");
                yield return new WaitForSeconds(2f);
                if (bossInteractionTheGolem.onFist)
                {
                    spinTimer = 0;
                    StartCoroutine(nameof(spinHandCoroutine));
                }
                else
                {
                    StartCoroutine(nameof(startFistSlamCoroutine));
                }

                StopCoroutine(nameof(raiseHandCoroutine));
            }

            yield return new WaitForFixedUpdate();
        }
    }

    void raiseHand()
    {
        Vector3 toMovePosition;
        if (bossInteractionTheGolem.onFist)
        {
            toMovePosition = Vector3.MoveTowards(leftHand.transform.position,
                new Vector3(initialHandPosition.x, initialHandPosition.y), handRaiseSpeed);
        }
        else
        {
            toMovePosition = Vector3.MoveTowards(leftHand.transform.position,
                new Vector3(leftHand.transform.position.x, initialHandPosition.y), handRaiseSpeed);
        }

        leftHand.transform.position = pixelPerfectCamera.RoundToPixel(toMovePosition);
    }

    IEnumerator spinHandCoroutine()
    {
        Debug.Log("Spin Hand");
        player.transform.SetParent(this.gameObject.transform.parent);
        leftHand.GetComponent<InteractionGolemHand>().spinning = true;
        player.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 35, ForceMode2D.Impulse);
        while (enabled)
        {
            spinTimer += Time.deltaTime;
            leftHand.transform.Rotate(0, 0, 250 * Time.deltaTime);
            if (spinTimer >= 2)
            {
                leftHand.transform.eulerAngles = new Vector3(0, 0, 0);
                leftHand.GetComponent<InteractionGolemHand>().spinning = false;
                StopCoroutine(nameof(spinHandCoroutine));
                startFirstStage();
            }

            yield return null;
        }
    }

    public void playerOnTopGolemFirstStage()
    {
        Debug.Log("Player On Golem");

        StopCoroutine(nameof(startFistSlamCoroutine));
        StopCoroutine(nameof(raiseHandCoroutine));
        StopCoroutine(nameof(fistSlamCoroutine));
    }

    IEnumerator waitForGolemToShakeOff()
    {
        yield return new WaitForSeconds(15f);
        Debug.Log("Start Shaking");
        player.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 150);
    }

    public void startSecondStage()
    {
        leftHand.SetActive(false);

        enableSecondStageColliders();
        StopCoroutine("startFistSlamCoroutine");
        StopCoroutine("raiseHandCoroutine");
        StopCoroutine("fistSlamCoroutine");

        StartCoroutine("secondStageCoroutine");
    }

    private void enableSecondStageColliders()
    {
        foreach (var collider2D1 in golemFirstStageCollision)
        {
            collider2D1.enabled = false;
        }

        foreach (var collider2D1 in golemSecondStageCollision)
        {
            collider2D1.enabled = true;
        }
    }

    IEnumerator secondStageCoroutine()
    {
        yield return new WaitForSeconds(10f);
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
        float randomWaitingTime = Random.Range(2, 3.5f);
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
        if (col.gameObject.CompareTag("Ground") && isBouncing && !shockWaveSpawned)
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
        StopCoroutine("bouncingAttackCoroutine");
    }

    public void getUp()
    {
        golemAnimator.SetBool("fallenOver", false);
        golemAnimator.SetTrigger("getUp");
        StartCoroutine(nameof(getUpCoroutine));
        Debug.Log("get up");
    }

    public IEnumerator getUpCoroutine()
    {
        yield return new WaitForSeconds(21f);
        if (bossInteractionTheGolem.secondStageCounter == 1)
        {
            bossStateManager.NextBossStage();
        }
        else
        {
            bouncingAttack();
        }
    }

    public void startThirdStage()
    {
        StartCoroutine(nameof(thirdStageCoroutine));
    }

    IEnumerator thirdStageCoroutine()
    {
        rb.bodyType = RigidbodyType2D.Static;
        uxInteraction.updateGolemFistIndicatorPosition(transform.position, player.transform.position, 5f);
        leftHand.SetActive(false);
        golemAnimator.SetTrigger("fallForwards");
        yield return new WaitForSeconds(2.5f);
        shakeGround();
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(nameof(commenceLazerFire));
    }

    private void shakeGround()
    {
        cameraShake.shakeCamera(0.35f, 0.4f);
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
            aimAtPlayer();
            int maxRangeDecreaser = 0;

            switch (bossInteractionTheGolem.finalStageCounter)
            {
                case 1:
                    maxRangeDecreaser = 1;
                    break;
            }

            int rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    int specialRand = Random.Range(0, 4 - maxRangeDecreaser);
                    switch (specialRand)
                    {
                        case 0:
                            fireLazerBall(0, 11f);
                            fireLazerBall(-4, 10.5f);
                            fireLazerBall(4, 10.5f);
                            fireLazerBall(-6);
                            fireLazerBall(6);
                            break;
                        default:
                            fireLazerBall(0, 12f);
                            fireLazerBall(-2);
                            fireLazerBall(2);
                            break;
                    }

                    break;
                case 1:
                    fireLazerBall(2, 12f);
                    fireLazerBall(-2, 12f);
                    break;
                default:
                    fireLazerBall();
                    break;
            }

            yield return new WaitForSeconds(0.5f);
            fireLazerBall(5);
            yield return new WaitForSeconds(0.5f);
            fireLazerBall(-5);
            yield return new WaitForSeconds(2f);
        }
    }

    void aimAtPlayer()
    {
        Vector3 dir = player.transform.position - lazerCannon.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lazerCannon.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void fireLazerBall(float angleOffset = 0, float ballSpeed = 10f)
    {
        int isLightBall = Random.Range(0, 2);
        Debug.Log("Fire");
        GameObject lazerBallInstance;

        switch (bossInteractionTheGolem.finalStageCounter)
        {
            case 1:
                ballSpeed += ballSpeed * 1.03f;
                break;
        }

        if (isLightBall == 1)
        {
            lazerBallInstance = Instantiate(lightLazerBall, lazerCannon.transform);
            lazerBallInstance.GetComponent<lazerBall>().isLightBall = true;
        }
        else
        {
            lazerBallInstance = Instantiate(darkLazerBall, lazerCannon.transform);
        }

        lazerBallInstance.GetComponent<lazerBall>().lazerBallSpeed = ballSpeed;
        lazerBallInstance.transform.eulerAngles =
            new Vector3(0, 0, lazerBallInstance.transform.eulerAngles.z + angleOffset);
        lazerBallInstance.transform.position = lazerCannon.transform.position;
    }

    public void finalStageHeadStrike()
    {
        bossInteractionTheGolem.finalStageCounter++;
        if (bossInteractionTheGolem.finalStageCounter != 2)
        {
            StartCoroutine(nameof(commenceLazerFire));
            return;
        }

        playerHasDefeatedGolem();
    }

    private void playerHasDefeatedGolem()
    {
        Debug.Log("Player has won");
    }
}