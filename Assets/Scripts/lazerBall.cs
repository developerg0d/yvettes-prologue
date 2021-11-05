using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lazerBall : MonoBehaviour
{
    [SerializeField] private float lazerBallSpeed = 2f;
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
        if (col.CompareTag("Player"))
        {
            if (col.gameObject.GetComponent<PlayerAttackScript>().isParrying)
            {
                transform.localScale = new Vector3(-1, 1, 1);
                lazerBallSpeed = (-lazerBallSpeed * 1.5f);
                beenParried = true;
                return;
            }

            Destroy(this.gameObject);
        }

        if (col.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}