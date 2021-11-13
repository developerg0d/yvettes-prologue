using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class lazerBall : MonoBehaviour
{
    [SerializeField] public float lazerBallSpeed = 10f;
    private float timer;
    public int bounceCounter;
    public bool beenParried;

    public bool isLightBall = false;

    public GameObject floatingEye;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        transform.SetParent(player.transform.parent);
        StartCoroutine("flying");
    }

    IEnumerator flying()
    {
        while (enabled)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            lazerBallSpeed = Mathf.Clamp(lazerBallSpeed + lazerBallSpeed * (distanceToPlayer / 100), 8, 20);
            timer += Time.deltaTime;
            transform.Translate(Vector3.right * lazerBallSpeed * Time.deltaTime);
            if (timer >= 100)
            {
                StopCoroutine(nameof(flying));
                Destroy(gameObject);
            }

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (col.gameObject.GetComponent<PlayerAttackScript>().isStabbing)
            {
                transform.eulerAngles = Vector3.zero;
                return;
            }

            if (col.gameObject.GetComponent<PlayerAttackScript>().isParrying)
            {
                transform.localScale = new Vector3(-2, 2, 2);
                lazerBallSpeed = (-lazerBallSpeed * 1.5f);
                beenParried = true;
                return;
            }

            Destroy(gameObject);
        }

        if (col.CompareTag("Ground") && !col.GetComponent<Collider2D>().isTrigger)
        {
            float randomFloat = Random.Range(0, 10);
            if (randomFloat == 0)
            {
                Instantiate(floatingEye, transform.position, floatingEye.transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}