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

    public Sprite[] leftSideSprites;
    public Sprite[] rightSideSprites;

    public GameObject leftSideLadder;
    public GameObject rightSideLadder;

    public SpriteRenderer leftSideRenderer;
    public SpriteRenderer rightSideRenderer;

    [SerializeField] private int leftSideHp = 2;
    [SerializeField] private int rightSideHp = 2;

    public bool canBeHit = true;

    void Start()
    {
        cols = GetComponentsInChildren<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        fistAnimator = GetComponent<Animator>();
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void setBottomOfFistPosition()
    {
        var fistColliderBounds = GetComponentInChildren<BoxCollider2D>().bounds;
        bottomOfFist.x = fistColliderBounds.center.x;
        bottomOfFist.y = fistColliderBounds.center.y - (fistColliderBounds.size.y / 2) - 0.5f;
    }

    public void groundExit()
    {
        fistAnimator.SetBool("fistInGround", false);
        canBeHit = false;
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
            rb.velocity = Vector2.zero;
            canBeHit = true;
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
                col.gameObject.transform.SetParent(gameObject.transform);
            }
        }

        if (col.CompareTag("Sword") && canBeHit)
        {
            canBeHit = false;
            var currentPosition = transform.position;
            var playerPosition = col.transform.parent.transform.position;

            if (playerPosition.x > currentPosition.x)
            {
                fistGotHit(false);
            }
            else
            {
                fistGotHit(true);
            }
            // mainInteractionScript.golemHandHit();
        }
    }

    private void fistGotHit(bool hitFromTheLeft)
    {
        cameraShake.shakeCamera(0.1f, 0.1f);

        if (hitFromTheLeft)
        {
            leftSideHit();
            return;
        }

        rightSideHit();
    }

    private void leftSideHit()
    {
        if (leftSideHp <= 0)
        {
            return;
        }

        leftSideHp -= 1;
        leftSideRenderer.sprite = leftSideSprites[leftSideHp];
        if (leftSideHp == 0)
        {
            leftSideLadder.SetActive(true);
        }
    }

    private void rightSideHit()
    {
        if (rightSideHp <= 0)
        {
            return;
        }

        rightSideHp -= 1;
        rightSideRenderer.sprite = rightSideSprites[rightSideHp];
        if (rightSideHp == 0)
        {
            rightSideLadder.SetActive(true);
        }
    }
}