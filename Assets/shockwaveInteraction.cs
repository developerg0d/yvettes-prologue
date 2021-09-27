using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shockwaveInteraction : MonoBehaviour
{
    public bool beenParried;
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (col.gameObject.GetComponent<PlayerAttackScript>().isParrying)
            {
                parried();
            }
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
