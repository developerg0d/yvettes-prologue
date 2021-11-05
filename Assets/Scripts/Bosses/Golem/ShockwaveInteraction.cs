using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveInteraction : MonoBehaviour
{
    public bool beenParried;

    [SerializeField] private float shockwaveForce;
    private float timer;
    private Rigidbody2D rb;

    private Collider2D shockwaveCollider;

    private bool hasInteraction;
    private Vector2 previousVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        shockwaveCollider = GetComponentInChildren<Collider2D>();

        StartCoroutine("destroyOverTime");
    }

    IEnumerator destroyOverTime()
    {
        while (enabled)
        {
            timer += Time.deltaTime;
            if (timer > 5)
            {
                StopCoroutine("destroyOverTime");
                Destroy(this.gameObject);
            }

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player") && !hasInteraction)
        {
            hasInteraction = true;
            transform.position = new Vector3(col.gameObject.transform.position.x, transform.position.y);

            PlayerAttackScript playerAttackScript = col.gameObject.GetComponent<PlayerAttackScript>();
            if (playerAttackScript.isParrying)
            {
                float leftSideX = shockwaveCollider.bounds.center.x - (shockwaveCollider.bounds.size.x / 2);
                transform.position = new Vector3(leftSideX, transform.position.y);
                GetComponent<Animator>().speed = 0f;
                previousVelocity = rb.velocity;
                rb.velocity = Vector3.zero;
                playerAttackScript.playerParried();
                StartCoroutine(nameof(shockWaveParryCoroutine));
                return;
            }

            if (playerAttackScript.isDefending)
            {
                rb.velocity = Vector3.zero;
                playerAttackScript.playerDefended();
                col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * (shockwaveForce / 2));
                StartCoroutine(nameof(dispelShock), 0.25f);
                return;
            }

            playerAttackScript.playerDied();
            col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * (shockwaveForce));
            StartCoroutine(nameof(dispelShock), 0.75f);
        }
    }

    IEnumerator dispelShock(float dispelTime)
    {
        yield return new WaitForSeconds(dispelTime);
        Destroy(this.gameObject);
    }

    IEnumerator shockWaveParryCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        shockwaveParried();
    }

    void shockwaveParried()
    {
        transform.localScale = new Vector3(1, 1, 1);
        GetComponent<Animator>().speed = 0.5f;
        float leftSideX = shockwaveCollider.bounds.center.x + (shockwaveCollider.bounds.size.x / 2);
        transform.position = new Vector3(leftSideX, transform.position.y);
        beenParried = true;
        rb.velocity = -previousVelocity;
    }
}