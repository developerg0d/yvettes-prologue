using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    [SerializeField] private int leftSideHp = 2;
    [SerializeField] private int rightSideHp = 2;

    private bool hitPlayer;
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
        bottomOfFist.y = fistColliderBounds.center.y - (fistColliderBounds.size.y / 2) - 0.25f;
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

        if (col.gameObject.CompareTag("Ground") && (IsSlamming || hitPlayer))
        {
            if (!hitPlayer)
            {
                cameraShake.shakeCamera(0.3f, 0.1f);
            }

            rb.velocity = Vector2.zero;
            canBeHit = true;
            fistAnimator.SetBool("fistInGround", true);
            climbingHolds.SetActive(true);
            spawnCrater();
            IsSlamming = false;
        }

        if (col.gameObject.CompareTag("Player") && isSlamming)
        {
            StartCoroutine(nameof(slowTimeCoroutine));
            col.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            IsSlamming = false;
            hitPlayer = true;
        }
    }

    IEnumerator slowTimeCoroutine()
    {
        Time.timeScale = 0.1f;
        virtualCamera.m_Lens.OrthographicSize = 5;
        cameraShake.shakeCamera(0.05f, 0.15f);
        virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 1.75f;
        yield return new WaitForSeconds(0.05f);
        spawnCrater();
        yield return new WaitForSeconds(0.05f);
        Time.timeScale = 1f;
    }

    private void spawnCrater()
    {
        Debug.Log("Spawn Crater");
        setBottomOfFistPosition();
        Instantiate(fistCrater, bottomOfFist, Quaternion.identity);
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