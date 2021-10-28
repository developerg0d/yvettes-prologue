using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Space(10)] [SerializeField] private float indicatorTimeout = 0.5F;
    public bool playerRidingHand = false;

    public GameObject leftHand;
    public UxInteraction uxInteraction;

    private float spinTimer = 0f;

    public Transform shockWaveSpawnLocation;

    public GameObject shockWave;

    private bool shockWaveSpawned;
    private Rigidbody2D rb;

    public GameObject lazerCannon;

    public bool firstStage;

    public bool secondStage;
    public bool finalStage;

    public GameObject lazerBall;

    private bool isBouncing = false;

    public UnityEngine.U2D.PixelPerfectCamera pixelPerfectCamera;
    private BossInteractionTheGolem bossInteractionTheGolem;

    private Animator golemAnimator;

    void Start()
    {
        assignVariables();
        startFirstStage();
    }

    void assignVariables()
    {
        golemAnimator = this.GetComponent<Animator>();
        bossInteractionTheGolem = GetComponent<BossInteractionTheGolem>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
    }

    public void startFirstStage()
    {
        initialHandPosition = leftHand.transform.position;
        StartCoroutine("startFirstStageCoroutine");
    }

    IEnumerator startFirstStageCoroutine()
    {
        yield return new WaitForSeconds(2F);
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
            raiseHand();

            if (leftHand.transform.position.y >= pixelPerfectCamera.RoundToPixel(initialHandPosition).y &&
                leftHand.transform.position.x >= pixelPerfectCamera.RoundToPixel(initialHandPosition).x)
            {
                Debug.Log("Raised Hand");
                yield return new WaitForSeconds(2f);
                StopCoroutine("raiseHandCoroutine");
                if (bossInteractionTheGolem.onFist)
                {
                    spinTimer = 0;
                    StartCoroutine("spinHandCoroutine");
                }
                else
                {
                    StartCoroutine("startFistSlamCoroutine");
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    void raiseHand()
    {
        float step = handRaiseSpeed * Time.time;
        Vector3 toMovePosition = Vector3.MoveTowards(leftHand.transform.position,
            new Vector3(initialHandPosition.x, initialHandPosition.y), step);
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
                StopCoroutine("spinHand");
            }

            yield return null;
        }
    }

    public void playerOnTopGolemFirstStage()
    {
        Debug.Log("Player On Golem");

        StopCoroutine("startFistSlamCoroutine");
        StopCoroutine("raiseHandCoroutine");
        StopCoroutine("fistSlam");
        // StartCoroutine("waitForGolemToShakeOff");
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