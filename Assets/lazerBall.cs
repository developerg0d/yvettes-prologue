using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lazerBall : MonoBehaviour
{
    [SerializeField]
    private float lazerBallSpeed = 2f;
    private float timer;

    public bool beenParried;
    void Start()
    {
        StartCoroutine("flying");
    }

    IEnumerator flying()
    {
        while (enabled)
        {
            timer += Time.deltaTime;
            transform.Translate(Vector3.right * lazerBallSpeed * Time.deltaTime);
            if (timer >= 10)
            {
                StopCoroutine("flying");
                Destroy(this.gameObject);
            }
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col)
        {
            if (col.gameObject.tag == "Player")
            {
                if (col.gameObject.GetComponent<PlayerAttackScript>().isParrying)
                {
                    lazerBallSpeed = -lazerBallSpeed;
                    beenParried = true;
                    return;
                }
            }
            if (col.gameObject.tag == "Ground")
            {
                Destroy(this.gameObject);
            }
        }
    }
}
