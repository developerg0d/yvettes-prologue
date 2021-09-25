using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackControllerTheGolem : MonoBehaviour
{
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

    private bool slammingHand;

    private bool raisingHand;

    private bool canFollowPlayer = true;

    private GameObject player;

    public bool playerRidingHand = false;

    public UxInteraction uxInteraction;

    public Vector3 offset;

    private float spinTimer = 0f;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        initialHandPosition = leftHand.transform.position;
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
        rightHandRb = rightHand.GetComponent<Rigidbody2D>();
        StartCoroutine("battleStart");
    }

    IEnumerator battleStart()
    {
        canFollowPlayer = true;
        yield return new WaitForSeconds(3F);
        StartCoroutine("startFistSlam");
    }

    void moveHandToPlayer()
    {
        float step = handFollowSpeed * Time.deltaTime;
        Vector3 offsetPlayerPosition = getOffsetPlayerPosition();
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(offsetPlayerPosition.x, leftHand.transform.position.y), step);
    }

    void moveHandToInitialSlamPosition()
    {
        Vector3 offsetPlayerPosition = getOffsetPlayerPosition();
        float step = handRaiseSpeed * Time.deltaTime;
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(leftHand.transform.position.x, initialHandPosition.y, 0), step);
    }

    Vector3 getOffsetPlayerPosition()
    {
        Vector3 offSetPlayerPosition = player.transform.position;
        offSetPlayerPosition.x += offset.x;
        offSetPlayerPosition.y += offset.y;
        return offSetPlayerPosition;
    }
    IEnumerator fistSlam()
    {
        raisingHand = false;
        canFollowPlayer = false;
        slamHand();
        yield return new WaitForSeconds(5f);
        initialHandPosition.x = leftHand.transform.position.x;
        disableHandHolds();
        StartCoroutine("raiseHand");
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

    void slamHand()
    {
        leftHandRb.AddForce(Vector2.down * slammingForce, ForceMode2D.Impulse);
        enableHandHolds();
    }

    void Update()
    {
        if (raisingHand)
        {
            moveHandToInitialSlamPosition();
        }
        if (canFollowPlayer)
        {
            moveHandToPlayer();
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
            if (leftHand.transform.position.y == initialHandPosition.y)
            {
                raisingHand = false;
                yield return new WaitForSeconds(2f);
                StopCoroutine("raiseHand");
                if (playerRidingHand)
                {
                    StartCoroutine("spinHand");
                }
                else
                {
                    canFollowPlayer = true;
                    StartCoroutine("startFistSlam");
                }
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

                StartCoroutine("startFistSlam");
                spinTimer = 0;
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


}
