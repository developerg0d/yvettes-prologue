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
    private float handRaiseSpeed;

    private Rigidbody2D leftHandRb;
    private Rigidbody2D rightHandRb;

    private Vector3 initialHandPosition;

    private bool slammingHand;
    void Start()
    {
        initialHandPosition = leftHand.transform.position;
        leftHandRb = leftHand.GetComponent<Rigidbody2D>();
        rightHandRb = rightHand.GetComponent<Rigidbody2D>();
        StartCoroutine("fistSlam");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (slammingHand)
            {
                slammingHand = false;
                StopCoroutine("fistSlam");
            }
            else
            {
                slammingHand = true;
                StartCoroutine("fistSlam");
            }
        }
    }
    IEnumerator fistSlam()
    {
        slamHand();
        yield return new WaitForSeconds(0f);
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
                yield return new WaitForSeconds(1f);
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
