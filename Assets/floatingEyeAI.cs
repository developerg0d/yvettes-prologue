using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using Cache = UnityEngine.Cache;

public class floatingEyeAI : MonoBehaviour
{
    private GameObject player;
    private Animator floatingEyeAnimator;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float lookingUpdateTime;

    public CameraShake cameraShake;

    private bool beenHit;

    private Rigidbody2D rigidbody2D;

    private bool movingForwards = true;

    private float moveTimer;
    private bool hitPlayer;

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
            if (movingForwards)
            {
                transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.Translate(Vector3.right * -moveSpeed * Time.deltaTime);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator goBackwardsTransition()
    {
        StopCoroutine(nameof(move));
        moveSpeed = moveSpeed / 2;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(nameof(move));
        movingForwards = false;
        hitPlayer = false;
        yield return new WaitForSeconds(0.5f);
        moveSpeed = moveSpeed * 2;
        movingForwards = true;
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
        if (col.gameObject.CompareTag("Player") && !hitPlayer)
        {
            hitPlayer = true;
            StartCoroutine(nameof(goBackwardsTransition));
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Sword") && !beenHit)
        {
            Collider2D[] collider2D = new Collider2D[1];
            col.GetContacts(collider2D);
            foreach (var collider2D1 in collider2D)
            {
                if (collider2D1.CompareTag("FloatingEye"))
                {
                    beenHit = true;
                    cameraShake.shakeCamera(0.1f, 0.1f);
                    StartCoroutine(nameof(dying), player.GetComponent<PlayerMovement>().isLeft);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            floatingEyeAnimator.SetBool("isNearPlayer", false);
        }
    }

    private void OnTriggerStay2D(Collider2D col)
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
            rigidbody2D.AddForce(new Vector2(-1, 0.1f) * 40, ForceMode2D.Impulse);
        }
        else
        {
            rigidbody2D.AddForce(new Vector2(1, 0.1f) * 40, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}