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

    private bool canFollowPlayer = true;

    private GameObject player;

    public UxInteraction uxInteraction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        initialHandPosition = leftHand.transform.position;
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
        rightHandRb = rightHand.GetComponent<Rigidbody2D>();
        StartCoroutine("fistSlam");
        StartCoroutine("followPlayer");
    }

    IEnumerator followPlayer()
    {
        while (enabled)
        {
            if (canFollowPlayer)
            {
                moveHandToPlayer();
            }
            yield return null;
        }
    }
    void moveHandToPlayer()
    {
        float step = handFollowSpeed * Time.deltaTime;
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(player.transform.position.x, leftHand.transform.position.y), step);

    }
    IEnumerator fistSlam()
    {
        slamHand();
        canFollowPlayer = false;
        yield return new WaitForSeconds(5f);
        initialHandPosition.x = leftHand.transform.position.x;
        StartCoroutine("raiseHand");
    }

    void slamHand()
    {
        leftHandRb.AddForce(Vector2.down * slammingForce, ForceMode2D.Impulse);
    }

    IEnumerator raiseHand()
    {
        while (enabled)
        {
            moveHandToInitialSlamPosition();
            if (leftHand.transform.position.y == initialHandPosition.y && leftHand.transform.position.x == initialHandPosition.x)
            {
                canFollowPlayer = true;
                yield return new WaitForSeconds(2f);
                uxInteraction.updateGolemFistIndicatorPosition(leftHand.transform.position);
                yield return new WaitForSeconds(indicatorTimeout);
                StopCoroutine("raiseHand");
                StartCoroutine("fistSlam");
            }
            yield return new WaitForSeconds(0f);
        }
    }

    void moveHandToInitialSlamPosition()
    {
        float step = handRaiseSpeed * Time.deltaTime;
        leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, initialHandPosition, step);
    }

}
