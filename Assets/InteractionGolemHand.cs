using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHand : MonoBehaviour
{

    private BossInteractionTheGolem mainInteractionScript;
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            rb.velocity = Vector2.zero;
        }

        if (col.collider.tag == "Sword" && mainInteractionScript.canTakeDamage == true)
        {
            mainInteractionScript.golemHandHit();
        }
    }
}
