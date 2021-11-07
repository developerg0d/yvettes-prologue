using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class floatingEyeAI : MonoBehaviour
{
    private GameObject player;
    private Animator floatingEyeAnimator;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float lookingUpdateTime;

    private bool beenHit;

    private Rigidbody2D rigidbody2D;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        floatingEyeAnimator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(nameof(lookAtPlayer));
        StartCoroutine(nameof(move));
    }

    IEnumerator move()
    {
        while (enabled)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator lookAtPlayer()
    {
        while (enabled)
        {
            Vector3 playerMiddle = new Vector3(player.transform.position.x, player.transform.position.y - 0.25f, 0);
            Vector3 dir = playerMiddle - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            yield return lookingUpdateTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player") && col.gameObject.GetComponent<PlayerAttackScript>().isStabbing &&
            !beenHit)
        {
            beenHit = true;
            StartCoroutine(nameof(dying), player.GetComponent<PlayerMovement>().isLeft);
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            floatingEyeAnimator.SetBool("isNearPlayer", true);
        }
    }

    IEnumerator dying(bool hitFromLeft)
    {
        StopCoroutine(nameof(move));
        StopCoroutine(nameof(lookAtPlayer));
        if (hitFromLeft)
        {
            rigidbody2D.AddForce(Vector2.left * 8, ForceMode2D.Impulse);
        }
        else
        {
            rigidbody2D.AddForce(Vector2.right * 8, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            floatingEyeAnimator.SetBool("isNearPlayer", false);
        }
    }
}