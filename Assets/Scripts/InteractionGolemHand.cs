using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHand : MonoBehaviour
{

    private BossInteractionTheGolem mainInteractionScript;
    private Rigidbody2D rb;

    public GameObject world;
    public bool tooCloseToBoss;
    BoxCollider2D[] cols;
    public bool isMoving;

    private bool onFist;
    void Start()
    {
        cols = GetComponentsInChildren<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.otherCollider.tag == "Ground" && col.gameObject.tag == "Player")
        {
            col.gameObject.transform.SetParent(world.transform);
        }
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

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Player" && onFist)
        {
            onFist = false;
            col.gameObject.transform.SetParent(world.transform);
        }
        if (col.tag == "FistAvoid")
        {
            tooCloseToBoss = false;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            if (isOnGroundTag(col))
            {
                onFist = true;
                col.gameObject.transform.SetParent(this.gameObject.transform);
            }
        }
    }

    bool isOnGroundTag(Collider2D col)
    {
        foreach (BoxCollider2D item in cols)
        {
            if (item.tag == "Ground" && item.IsTouching(col))
            {
                return true;
            }
        }
        return false;
    }
    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "FistAvoid")
        {
            tooCloseToBoss = true;
        }
    }
}
