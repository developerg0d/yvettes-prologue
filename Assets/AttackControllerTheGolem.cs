using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackControllerTheGolem : MonoBehaviour
{
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

    private bool canFollow = true;

    private GameObject player;



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
            if (canFollow)
            {
                float step = handFollowSpeed * Time.deltaTime;
                leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, new Vector3(player.transform.position.x, leftHand.transform.position.y), step);
            }
            yield return null;
        }
    }
    IEnumerator fistSlam()
    {
        slamHand();
        canFollow = false;
        yield return new WaitForSeconds(5f);
        initialHandPosition.x = leftHand.transform.position.x;
        StartCoroutine("raiseHand");
    }

    IEnumerator raiseHand()
    {
        while (enabled)
        {
            float step = handRaiseSpeed * Time.deltaTime;
            leftHand.transform.position = Vector3.MoveTowards(leftHand.transform.position, initialHandPosition, step);
            if (leftHand.transform.position.y == initialHandPosition.y && leftHand.transform.position.x == initialHandPosition.x)
            {
                canFollow = true;
                yield return new WaitForSeconds(2.5f);
                StartCoroutine("fistSlam");
                StopCoroutine("raiseHand");
            }
            yield return new WaitForSeconds(0f);
        }
    }

    void slamHand()
    {
        leftHandRb.AddForce(Vector2.down * slammingForce, ForceMode2D.Impulse);
    }
}
