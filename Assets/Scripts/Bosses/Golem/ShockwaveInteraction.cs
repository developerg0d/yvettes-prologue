using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveInteraction : MonoBehaviour
{
    public bool beenParried;

    [SerializeField] private float shockwaveForce;
    private float timer;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        if (col.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector3.zero;
            PlayerAttackScript playerAttackScript = col.gameObject.GetComponent<PlayerAttackScript>();
            if (playerAttackScript.isParrying)
            {
                Debug.Log("parried");
                parried();
                return;
            }

            if (playerAttackScript.isDefending)
            {
                Debug.Log("defended");
                col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * (shockwaveForce / 2));
                StartCoroutine(nameof(dispelShock));
                return;
            }

            Debug.Log("death");
            col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * (shockwaveForce));
            StartCoroutine(nameof(dispelShock));
        }
    }

    IEnumerator dispelShock()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }

    void parried()
    {
        beenParried = true;
        float previousVelocity = rb.velocity.x;
        Debug.Log(previousVelocity);
        transform.localScale = new Vector3(1, 1, 1);
        rb.velocity = Vector2.left * (previousVelocity * 3);
    }
}