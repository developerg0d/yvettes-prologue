using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneTemplate;
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

    public GameObject fistCrater;
    private Vector3 bottomOfFist;

    public bool IsSlamming
    {
        get => isSlamming;
        set => isSlamming = value;
    }

    public bool spinning = false;

    private bool onFist;

    public CameraShake cameraShake;

    private Animator fistAnimator;

    void Start()
    {
        setBottomOfFistPosition();
        fistCrater.transform.position = bottomOfFist;
        cols = GetComponentsInChildren<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        fistAnimator = GetComponent<Animator>();
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void setBottomOfFistPosition()
    {
        var fistColliderBounds = GetComponentInChildren<BoxCollider2D>().bounds;
        bottomOfFist.x = fistColliderBounds.center.x;
        bottomOfFist.y = fistColliderBounds.center.y - (fistColliderBounds.size.y / 2);
    }

    public void groundExit()
    {
        fistAnimator.SetBool("fistInGround", false);

        climbingHolds.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (spinning)
        {
            return;
        }

        if (col.gameObject.CompareTag("Ground") && IsSlamming)
        {
            cameraShake.shakeCamera(0.3f, 0.1f);
            fistAnimator.SetBool("fistInGround", true);
            spawnCrater();
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

            spawnCrater();
            cameraShake.shakeCamera(0.6f, 0.1f);
        }

        if (col.collider.CompareTag("Sword") && mainInteractionScript.canTakeDamage == true)
        {
            mainInteractionScript.golemHandHit();
        }
    }

    private void spawnCrater()
    {
        setBottomOfFistPosition();
        Debug.Log(bottomOfFist);
        Instantiate(fistCrater, bottomOfFist, Quaternion.identity);
        Debug.Log(bottomOfFist);
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