using System;
using System.Collections;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteraction : MonoBehaviour
{
    private Rigidbody2D rb;
    public Vector2 playerVelocity;

    private UxInteraction uxInteraction;
    private PlayerAttackScript playerAttackScript;
    private PlayerStats playerStats;

    private SoundManager soundManager;
    private Animator playerAnimator;

    public bool upThrustReady;
    public bool isScalingWall;
    public bool grounded;
    public bool onLadder;
    public bool canClimb;
    public bool canHit;
    public bool onSideLadder;
    public CinemachineVirtualCamera virtualCamera;

    private SpriteRenderer playerRenderer;
    public bool teleporting;
    private CameraShake cameraShake;

    public Material[] materials;
    public bool spawnedBoss;
    private PlayerMovement playerMovement;

    public BossStateManager bossStateManager;

    public Checkpoint lastCheckpoint;
    public GolemAttackController golemAttackController;
    private bool dead;

    private void Start()
    {
        assignVariables();
    }

    private void assignVariables()
    {
        playerRenderer = GetComponentInChildren<SpriteRenderer>();
        soundManager = GameObject.FindWithTag("AudioSource").GetComponent<AudioSource>().GetComponent<SoundManager>();
        cameraShake = FindObjectOfType<CameraShake>();
        uxInteraction = FindObjectOfType<UxInteraction>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttackScript = GetComponent<PlayerAttackScript>();
        playerAnimator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Golem") && !spawnedBoss)
        {
            spawnedBoss = true;
            bossStateManager.StartBossStages();
        }

        if (col.CompareTag("LazerBall"))
        {
            if (col.GetComponent<lazerBall>().isLightBall)
            {
                if (playerAttackScript.isDefending)
                {
                    rb.AddForce(Vector2.left * 50);
                    playerAttackScript.playerDefended(0);
                    return;
                }
            }

            if (playerAttackScript.isParrying)
            {
                rb.AddForce(Vector2.left * 25);
                playerAttackScript.playerParried();
                return;
            }

            playerHit();
        }


        if (col.CompareTag("ClimbingSection"))
        {
            bool facingRight = rb.velocity.x > 0;
            uxInteraction.enableClimbingIndicator(facingRight);
        }
    }

    private void LateUpdate()
    {
        playerAnimator.SetBool("onSideLadder", onSideLadder);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("LadderEnd") && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)))
        {
            Debug.Log("Ladder Jump");

            playerAnimator.SetBool("isClimbing", false);
            canClimb = false;
            Vector3 endPosition = transform.position;
            endPosition.y += 2;
            playerAnimator.SetTrigger("climbJump");
            StartCoroutine(nameof(exitLadderCoroutine), endPosition);
        }

        if (col.CompareTag("Checkpoint"))
        {
            if (Input.GetKey(KeyCode.S) && !teleporting)
            {
                teleporting = true;
                Debug.Log("Teleport");
                Checkpoint checkpoint = col.GetComponent<Checkpoint>();
                if (checkpoint.activated)
                {
                    teleport(checkpoint);
                }
            }
        }

        if (col.CompareTag("ScalingWall"))
        {
            rb.gravityScale = 0.7f;
            isScalingWall = true;
        }

        if (col.CompareTag("SideLadder"))
        {
            if (playerMovement.horizontalMovement > 0f)
            {
                rb.gravityScale = 0.3f;
                onSideLadder = true;
            }

            if (playerMovement.horizontalMovement <= 0)
            {
                rb.gravityScale = 1f;
            }
        }

        if (col.CompareTag("Ladder"))
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                onLadder = true;
                playerAnimator.SetBool("onLadder", true);
            }

            if (rb.gravityScale != 0)
            {
                rb.gravityScale = 0;
            }

            canClimb = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("ClimbingSection"))
        {
            uxInteraction.disableClimbingIndicator();
        }

        if (col.CompareTag("ScalingWall"))
        {
            rb.gravityScale = 1.0f;
            isScalingWall = false;
        }

        if (col.tag == "SideLadder")
        {
            onSideLadder = false;
            rb.gravityScale = 1;
        }

        if (col.CompareTag("Ladder"))
        {
            onLadder = false;
            playerAnimator.SetBool("onLadder", false);
            rb.gravityScale = 1;
            canClimb = false;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Checkpoint"))
        {
            lastCheckpoint = col.gameObject.GetComponent<Checkpoint>();
        }

        if (col.gameObject.CompareTag("Dead") && !dead)
        {
            playerCrushed();
        }

        if (col.gameObject.CompareTag("FloatingEye") && canHit)
        {
            playerHit();
        }

        if (col.collider.CompareTag("GolemHand"))
        {
            InteractionGolemHand golemHandScript = col.gameObject.GetComponent<InteractionGolemHand>();

            if (golemHandScript.IsSlamming && playerStats.CanDie)
            {
                playerCrushed();
            }
        }

        if (col.gameObject.CompareTag("Golem") &&
            col.gameObject.GetComponent<GolemAttackController>().isFalling)
        {
            rb.AddForce(Vector2.left * 200);
            playerAttackScript.mediumCameraShake();
            playerCrushed();
        }

        if (col.gameObject.CompareTag("ScalingWall"))
        {
            rb.velocity = rb.velocity / 4;
        }

        if (col.gameObject.CompareTag("Ground") || col.otherCollider.CompareTag("Ground") && !isOnLadder() &&
            !isScalingWall)
        {
            Debug.Log(playerVelocity);
            StopCoroutine(nameof(inAirCoroutine));
            if (!playerStats.CanDie)
            {
                return;
            }

            if (playerVelocity.y < -7.5f)
            {
                playerCrushed();
                Debug.Log("Death by Fall Damage");
            }
        }
    }

    private void teleport(Checkpoint checkpoint)
    {
        if (checkpoint.checkpoints.Length <= 1)
        {
            teleporting = false;
            return;
        }

        uxInteraction.teleportationUx.SetActive(true);
        disableControls();

        uxInteraction.leftTeleport.onClick.AddListener(
            delegate { leftTeleport(checkpoint); });

        uxInteraction.rightTeleport.onClick.AddListener(
            delegate { rightTeleport(checkpoint); });
    }

    void disableControls()
    {
        playerAttackScript.canAttack = false;
        playerMovement.canMove = false;
    }

    void leftTeleport(Checkpoint checkpoint)
    {
        int teleportIndex = checkpoint.currentIndex - 1;
        Vector2 teleportPosition;
        if (checkpoint.checkpoints[teleportIndex])
        {
            teleportPosition = checkpoint.checkpoints[teleportIndex].transform.position;
        }
        else
        {
            teleportPosition = checkpoint.checkpoints[checkpoint.checkpoints.Length - 1].transform.position;
        }

        transform.position = teleportPosition;
        completeTeleportation();
        playerMovement.spawnEffect();
    }

    void rightTeleport(Checkpoint checkpoint)
    {
        int teleportIndex = checkpoint.currentIndex + 1;
        Vector2 teleportPosition;
        if (checkpoint.checkpoints.Length > teleportIndex)
        {
            teleportPosition = checkpoint.checkpoints[teleportIndex].transform.position;
        }
        else
        {
            teleportPosition = checkpoint.checkpoints[0].transform.position;
        }

        transform.position = teleportPosition;
        completeTeleportation();
    }

    void completeTeleportation()
    {
        uxInteraction.rightTeleport.onClick.RemoveAllListeners();
        uxInteraction.leftTeleport.onClick.RemoveAllListeners();
        uxInteraction.teleportationUx.SetActive(false);
        playerAttackScript.canAttack = true;
        playerMovement.canMove = true;
        teleporting = false;
    }

    bool isOnLadder()
    {
        return onLadder || onSideLadder;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if ((col.gameObject.CompareTag("Ground") ||
             col.otherCollider.CompareTag("Ground")) && !isOnLadder())
        {
            grounded = false;
            StartCoroutine(nameof(inAirCoroutine));
        }
    }

    IEnumerator exitLadderCoroutine(Vector3 endPosition)
    {
        while (enabled)
        {
            if (transform.position.y < endPosition.y)
            {
                rb.velocity = new Vector3(playerMovement.isLeft == true ? -1 : 1, 2f, 0) * 3f;
            }
            else
            {
                StopCoroutine(nameof(exitLadderCoroutine));
            }

            yield return null;
        }
    }

    IEnumerator inAirCoroutine()
    {
        while (!grounded)
        {
            playerVelocity = rb.velocity;
            yield return new WaitForFixedUpdate();
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if ((col.gameObject.CompareTag("Ground") || col.otherCollider.CompareTag("Ground")) && !isOnLadder() &&
            !isScalingWall)
        {
            rb.gravityScale = 1f;
            upThrustReady = true;
            grounded = true;
        }
    }

    private void playerHit(int damageHit = 1)
    {
        StartCoroutine(nameof(changeMaterial));
        cameraShake.shakeCamera(0.3f, 0.2f);
        rb.AddForce(Vector2.left * 100);
        playerStats.CurrentHp -= damageHit;
        if (playerStats.CurrentHp > 0)
        {
            uxInteraction.updatePlayerHpBar(playerStats.CurrentHp);
        }

        if (playerStats.CurrentHp == 0)
        {
            playerCrushed();
        }
        else
        {
            if (soundManager.fxOn)
            {
                soundManager.playHitSound();
            }
        }

        StartCoroutine(nameof(hitDelay));
        Debug.Log("Hit");
    }

    IEnumerator hitDelay()
    {
        canHit = false;
        yield return new WaitForSeconds(1f);
        canHit = true;
    }

    public void playerRespawn()
    {
        playerAnimator.SetTrigger("respawned");
        dead = false;
        playerMovement.canMove = true;
        playerAttackScript.canAttack = true;
        transform.position = lastCheckpoint.transform.position;
        soundManager.playMainTheme();
        virtualCamera.m_Lens.OrthographicSize = 10;
        virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 3f;
        playerStats.CurrentHp = playerStats.MaxHp;
        uxInteraction.updatePlayerHpBar(playerStats.CurrentHp);
    }

    public void playerCrushed()
    {
        if (soundManager.fxOn)
        {
            soundManager.playBigHitSound();
        }

        if (bossStateManager.isFightingBoss)
        {
            golemAttackController.StopAllCoroutines();
        }

        dead = true;
        playerAnimator.SetTrigger("dead");
        enabled = false;
        uxInteraction.disableUiOnDeath();
        disableControls();
        playerStats.CurrentHp = 0;
        uxInteraction.updatePlayerHpBar(0);
        Debug.Log("Player Dead");
    }

    IEnumerator changeMaterial()
    {
        playerRenderer.material = materials[1];
        yield return new WaitForSeconds(0.2f);
        playerRenderer.material = materials[0];
    }
}