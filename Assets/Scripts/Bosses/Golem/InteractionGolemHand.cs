using System;
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

    private bool isSlamming;

    public GameObject fistCrater;
    private Vector3 bottomOfFist;
    public Material[] materials;
    public GameObject floatingEye;

    public GameObject hpUx;
    public Camera mainCamera;
    public Image bossHandHpUx;

    public bool IsSlamming
    {
        get => isSlamming;
        set => isSlamming = value;
    }

    public bool spinning = false;

    private bool onFist;

    public CameraShake cameraShake;

    private Animator fistAnimator;

    public Sprite[] sprites;

    public GameObject leftSideLadder;
    public GameObject rightSideLadder;
    private GolemAttackController golemAttackController;
    public SpriteRenderer spriteRenderer;
    public CinemachineVirtualCamera virtualCamera;
    [SerializeField] private int golemHandHp = 300;
    private bool hitPlayer;
    public bool canBeHit = true;

    private int hitLevel;

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

        if ((col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Checkpoint") ||
             col.gameObject.CompareTag("Crater")) &&
            IsSlamming)
        {
            hitGround();
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


    public void resetGolemHand()
    {
        recoverAllHp();
    }

    private void OnGUI()
    {
        if (hpUx.gameObject.activeSelf)
        {
            Vector3 uxLocation = new Vector3(transform.position.x, transform.position.y + 5);

            hpUx.transform.position = mainCamera.WorldToScreenPoint(uxLocation);
        }
    }

    private void hitGround()
    {
        if (!hitPlayer)
        {
            cameraShake.shakeCamera(0.3f, 0.1f);
        }

        if (soundManager.fxOn)
        {
            soundManager.playMassiveHitSound();
        }

        hpUx.SetActive(true);
        rb.velocity = Vector2.zero;
        canBeHit = true;
        fistAnimator.SetBool("fistInGround", true);
        spawnCrater();
        IsSlamming = false;
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
        if (col.CompareTag("Player"))
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
            StartCoroutine(nameof(hitDelay));
            fistGotHit(30);
        }
    }

    IEnumerator hitDelay()
    {
        yield return new WaitForSeconds(0.2f);
        canBeHit = true;
    }

    public void recoverAllHp()
    {
        spriteRenderer.sprite = sprites[0];
        climbingHolds.SetActive(false);
        golemHandHp = 300;
        hitLevel = 0;
        updateGolemHandHp(golemHandHp);
    }

    private void fistGotHit(int damage)
    {
        cameraShake.shakeCamera(0.1f, 0.1f);

        StartCoroutine(nameof(changeMaterial));
        sideHit(damage);
    }

    IEnumerator changeMaterial()
    {
        spriteRenderer.material = materials[1];
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material = materials[0];
    }

    private void sideHit(int damage)
    {
        if (golemHandHp > 0)
        {
            golemHandHp -= damage;
            updateGolemHandHp(golemHandHp);
        }

        if (IsBetween(golemHandHp, 101, 200) && hitLevel != 1)
        {
            hitLevel = 1;
            majorHit(1);
        }
        else if (IsBetween(golemHandHp, 1, 100) && hitLevel != 2)
        {
            hitLevel = 2;
            majorHit(2);
        }
        else if (golemHandHp <= 0 && hitLevel != 3)
        {
            hitLevel = 3;
            majorHit(3);
            climbingHolds.SetActive(true);
        }
        else
        {
            if (soundManager.fxOn)
            {
                soundManager.playHitSound();
            }
        }
    }

    void updateGolemHandHp(int currentHp)
    {
        float convertedHp = currentHp / 300f;
        bossHandHpUx.fillAmount = convertedHp;
    }

    void majorHit(int spriteIndex)
    {
        golemAttackController.retractHandInstantly();
        if (soundManager.fxOn)
        {
            soundManager.playBigHitSound();
        }

        spriteRenderer.sprite = sprites[spriteIndex];
    }

    public bool IsBetween(double testValue, double bound1, double bound2)
    {
        return (testValue >= Math.Min(bound1, bound2) && testValue <= Math.Max(bound1, bound2));
    }
}