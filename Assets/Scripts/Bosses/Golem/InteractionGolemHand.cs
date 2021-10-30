using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHand : MonoBehaviour
{
    private BossInteractionTheGolem mainInteractionScript;

    private Rigidbody2D rb;

    public GameObject world;
    public bool tooCloseToBoss;
    public GameObject climbingHolds;
    BoxCollider2D[] cols;
    public bool isMoving;

    private bool isSlamming;

    public bool IsSlamming
    {
        get => isSlamming;
        set => isSlamming = value;
    }

    public bool spinning = false;

    private bool onFist;

    public CameraShake cameraShake;

    void Start()
    {
        cols = GetComponentsInChildren<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            climbingHolds.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (spinning)
        {
            return;
        }

        if (col.gameObject.CompareTag("Ground"))
        {
            cameraShake.shakeCamera(0.3f, 0.1f);
            climbingHolds.SetActive(true);
            IsSlamming = false;
        }

        if (col.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector2.zero;
            if (!isSlamming)
            {
                return;
            }

            cameraShake.shakeCamera(0.6f, 0.1f);
        }

        if (col.collider.CompareTag("Sword") && mainInteractionScript.canTakeDamage == true)
        {
            mainInteractionScript.golemHandHit();
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            Debug.Log("off fist");
            mainInteractionScript.onFist = false;
            col.gameObject.transform.SetParent(world.transform);
        }

        if (col.tag == "FistAvoid")
        {
            tooCloseToBoss = false;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (spinning)
        {
            return;
        }

        if (col.tag == "Player")
        {
            PlayerMovement playerMovement = col.GetComponent<PlayerMovement>();
            if (playerMovement.canClimb == false)
            {
                Debug.Log("on fist");
                mainInteractionScript.onFist = true;
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