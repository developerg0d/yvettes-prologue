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

    private bool raisingHand;

    public bool canFollowPlayer = true;

    private GameObject player;

    public bool playerRidingHand = false;

    public UxInteraction uxInteraction;

    public Vector3 offset;
    public bool returningToOriginalPosition;

    public bool useOffset;

    public bool jumping;

    private float spinTimer = 0f;

    // IEnumerator raiseHandCoroutine;
    // IEnumerator spinHandCoroutine;
    // IEnumerator returnToOriginalPositionCoroutine;
    private Rigidbody2D rb;

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
        yield return new WaitForSeconds(3F);
        StartCoroutine("bouncingAttack");
        // StartCoroutine("startFistSlam");
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
            // rb.AddForce(Vector2.left * golemJumpForwardsForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator bouncingAttack()
    {
        while (enabled)
        {
            jumping = true;
            yield return new WaitForSeconds(7f);
        }
    }
}
