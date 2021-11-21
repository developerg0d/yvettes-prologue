using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class InteractionGolemHand : MonoBehaviour
{
    private BossInteractionTheGolem mainInteractionScript;

    private Rigidbody2D rb;

    public GameObject world;
    public GameObject climbingHolds;
    public bool isMoving;

    private bool isSlamming;

    public Sprite fullCrystal;
    public Sprite emptyCrystal;
    public GameObject[] uxHp;
    public GameObject fistCrater;
    private Vector3 bottomOfFist;
    public Material[] materials;
    public GameObject floatingEye;

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
    private GolemAttackController golemAttackController;
    public SpriteRenderer leftSideRenderer;
    public SpriteRenderer rightSideRenderer;
    public CinemachineVirtualCamera virtualCamera;
    [SerializeField] private int leftSideHp = 2;
    [SerializeField] private int rightSideHp = 2;
    private bool hitPlayer;
    public bool canBeHit = true;

    private SoundManager soundManager;

    void Start()
    {
        soundManager = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<SoundManager>();

        golemAttackController = GetComponentInParent<GolemAttackController>();
        rb = GetComponent<Rigidbody2D>();
        fistAnimator = GetComponent<Animator>();
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void setBottomOfFistPosition()
    {
        var fistColliderBounds = GetComponentInChildren<BoxCollider2D>().bounds;
        bottomOfFist.x = fistColliderBounds.center.x;
        bottomOfFist.y = fistColliderBounds.center.y - (fistColliderBounds.size.y / 2) - 0.30f;
    }

    public void groundExit()
    {
        var fistColliderBounds = GetComponentInChildren<BoxCollider2D>().bounds;

        if (!mainInteractionScript.onFist)
        {
            spawnLeftMob(fistColliderBounds);
            spawnRightMob(fistColliderBounds);
        }

        fistAnimator.SetBool("fistInGround", false);
        canBeHit = false;
        climbingHolds.SetActive(false);
    }

    void spawnLeftMob(Bounds fistColliderBounds)
    {
        Vector3 leftSideVector =
            new Vector3(transform.position.x - (fistColliderBounds.size.x / 2), leftSideLadder.transform.position.y, 0);
        GameObject floatingEyeInstance = Instantiate(floatingEye);
        floatingEyeInstance.transform.position = leftSideVector;
    }

    void spawnRightMob(Bounds fistColliderBounds)
    {
        Vector3 leftSideVector =
            new Vector3(transform.position.x + (fistColliderBounds.size.x / 2), leftSideLadder.transform.position.y, 0);
        GameObject floatingEyeInstance = Instantiate(floatingEye);
        floatingEyeInstance.transform.position = leftSideVector;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (spinning)
        {
            return;
        }

        if (col.gameObject.CompareTag("Player") && col.otherCollider.CompareTag("Ground"))
        {
            Debug.Log("on fist");
            mainInteractionScript.onFist = true;
            col.gameObject.transform.SetParent(gameObject.transform);
        }

        if ((col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Checkpoint")) &&
            IsSlamming)
        {
            if (!hitPlayer)
            {
                cameraShake.shakeCamera(0.3f, 0.1f);
            }

            if (soundManager.fxOn)
            {
                soundManager.playMassiveHitSound();
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
            if (soundManager.fxOn)
            {
                soundManager.playMassiveHitSound();
            }

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
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (spinning)
        {
            return;
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

    public void recoverAllHp()
    {
        leftSideHp = 2;
        rightSideHp = 2;
        restoreHandHpUx();
        leftSideRenderer.sprite = leftSideSprites[0];
        rightSideRenderer.sprite = rightSideSprites[0];
        leftSideLadder.SetActive(false);
        rightSideLadder.SetActive(false);
    }

    private void fistGotHit(bool hitFromTheLeft)
    {
        cameraShake.shakeCamera(0.1f, 0.1f);
        if (soundManager.fxOn)
        {
            soundManager.playBigHitSound();
        }

        golemAttackController.retractHandInstantly();
        StartCoroutine(nameof(changeMaterial));
        if (hitFromTheLeft)
        {
            leftSideHit();
            return;
        }

        rightSideHit();
    }

    IEnumerator changeMaterial()
    {
        leftSideRenderer.material = materials[1];
        rightSideRenderer.material = materials[1];
        yield return new WaitForSeconds(0.1f);
        leftSideRenderer.material = materials[0];
        rightSideRenderer.material = materials[0];
    }


    private void leftSideHit()
    {
        if (leftSideHp <= 0)
        {
            return;
        }

        updateHandHpUx(leftSideHp, true);
        leftSideHp -= 1;
        leftSideRenderer.sprite = leftSideSprites[leftSideHp];
        if (leftSideHp == 0)
        {
            leftSideLadder.SetActive(true);
        }
    }

    private void restoreHandHpUx()
    {
        GameObject parentHpUx = uxHp[0].transform.parent.gameObject;
        Image[] uxHpImages = parentHpUx.GetComponentsInChildren<Image>();
        foreach (var uxImage in uxHpImages)
        {
            if (uxImage.sprite == emptyCrystal)
            {
                uxImage.sprite = fullCrystal;
            }
        }
    }

    private void updateHandHpUx(int currentHp, bool isLeft)
    {
        GameObject handSideHpUx = uxHp[isLeft ? 0 : 1];
        Image[] uxImages = handSideHpUx.GetComponentsInChildren<Image>();
        uxImages[currentHp - 1].sprite = emptyCrystal;
    }

    private void rightSideHit()
    {
        if (rightSideHp < 0)
        {
            return;
        }

        updateHandHpUx(rightSideHp, false);

        rightSideHp -= 1;
        rightSideRenderer.sprite = rightSideSprites[rightSideHp];
        if (rightSideHp == 0)
        {
            rightSideLadder.SetActive(true);
        }
    }
}