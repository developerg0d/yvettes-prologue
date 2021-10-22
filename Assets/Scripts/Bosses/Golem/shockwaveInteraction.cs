using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shockwaveInteraction : MonoBehaviour
{
    public bool beenParried;

    [SerializeField]
    private float shockwaveForce;
    private float timer;
    void Start()
    {
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
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
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
                col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * shockwaveForce);
                Destroy(this.gameObject);
                return;
            }
            Debug.Log("death");
            col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * (shockwaveForce * 2));
            Destroy(this.gameObject);
        }

        if (col.gameObject.tag == "Golem" && beenParried)
        {
            Debug.Log("PARRIED");
        }
    }

    void parried()
    {
        beenParried = true;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        float previousVelocity = rb.velocity.x;
        rb.velocity = Vector2.zero;
        Debug.Log(previousVelocity);
        transform.localScale = new Vector3(1, 1, 1);
        rb.velocity = Vector2.left * (previousVelocity * 3);
    }
}
