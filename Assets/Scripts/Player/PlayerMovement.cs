using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float climbingSpeed;
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float jumpPower = 30f;
    private Animator playerAnimator;
    private SpriteRenderer renderer;
    [SerializeField] private Transform playerSprite;

    float horizontalMovement;
    private PlayerAttackScript playerAttackScript;

    private bool moving;
    public Material[] materials;
    public bool grounded;
    private Rigidbody2D rb;

    private float dashTimer;
    private bool canHit = true;
    private bool canTurn;
    private bool canDash = true;
    [SerializeField] float dashDelay = 3.0f;

    public GameObject world;

    public bool canClimb;

    private bool upThrustReady;

    private bool isScalingWall;

    private PlayerStats playerStats;
    public CameraShake cameraShake;

    public bool isLeft;

    private bool onLadder;

    private float exitLadderTimer;

    private bool isJumpingUpLadder;

    public Vector2 playerVelocity;
    private bool isClimbing;
    public bool canMove = true;
    bool onSideLadder;
    public UxInteraction uxInteraction;
    public GameObject playerSpawnEffect;

    public AudioClip beenHitAudio;

    private SoundManager soundManager;

    void Awake()
    {
        soundManager = GameObject.FindWithTag("AudioSource").GetComponent<AudioSource>().GetComponent<SoundManager>();
        GameObject spawnEffect = Instantiate(playerSpawnEffect, transform.position, transform.rotation);
        spawnEffect.transform.SetParent(transform);
        StartCoroutine(nameof(playerSpawnEffectCoroutine), spawnEffect);
    }

    IEnumerator playerSpawnEffectCoroutine(GameObject spawnEffect)
    {
        yield return new WaitForSeconds(1f);
        Destroy(spawnEffect);
    }

    void Start()
    {
        renderer = GetComponentInChildren<SpriteRenderer>();
        playerStats = GetComponent<PlayerStats>();
        playerAttackScript = GetComponent<PlayerAttackScript>();
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        playerAnimator.SetBool("isScalingWall", isScalingWall);
        playerAnimator.SetBool("onSideLadder", onSideLadder);
        playerAnimator.SetBool("isSideLadderClimbing", isClimbing);
        playerAnimator.SetBool("isMoving", moving);
        playerAnimator.SetBool("onGround", grounded);
    }

    void Update()
    {
        if (canMove)
        {
            MovementControls();
        }
    }

    bool isOnLadder()
    {
        return onLadder || onSideLadder;
    }

    void MovementControls()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.W) && !grounded && upThrustReady)
        {
            upThrustReady = false;
            playerAnimator.SetBool("upthrust", true);
            swordUpThrust();
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            playerAnimator.SetBool("upthrust", false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && !playerAttackScript.isDefending && canDash)
        {
            float dashingAnimationDirection = isLeft == true ? 1 : 0;
            playerAnimator.SetFloat("dashDirection", dashingAnimationDirection);
            dashPlayer(0);
        }
        else if (Input.GetKeyDown(KeyCode.E) && !playerAttackScript.isDefending && canDash)
        {
            float dashingAnimationDirection = isLeft == true ? 0 : 1;
            playerAnimator.SetFloat("dashDirection", dashingAnimationDirection);
            dashPlayer(1);
        }

        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)) && canClimb)
        {
            isClimbing = true;
            playerAnimator.SetBool("isClimbing", true);
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) && canClimb)
        {
            isClimbing = false;
            playerAnimator.SetBool("isClimbing", false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            jump();
        }
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            PhysicsMovement();
        }
    }

    void PhysicsMovement()
    {
        horizontalMovement = Input.GetAxis("HorizontalMoving");
        isPlayerMoving(horizontalMovement);
        float verticalMovement = Input.GetAxis("VerticalMoving");
        if (canClimb && verticalMovement != 0)
        {
            climb(verticalMovement);
        }

        if (onSideLadder && horizontalMovement != 0)
        {
            movePlayerDiagonally(horizontalMovement);
            return;
        }

        if (horizontalMovement != 0)
        {
            movePlayerHorizontally(horizontalMovement);
        }
    }

    void isPlayerMoving(float horizontalMovement)
    {
        moving = horizontalMovement != 0;
    }

    void movePlayerDiagonally(float horizontalMovement)
    {
        Vector2 horizontalDirection =
            horizontalMovement <= 0 ? new Vector2(0, 0.5f) : new Vector2(1, 0.5f);
        rb.velocity = horizontalDirection * (horizontalSpeed * 0.1f);
    }

    void movePlayerHorizontally(float horizontalMovement)
    {
        updatePlayerDirection(horizontalMovement);
        Vector2 horizontalDirection =
            horizontalMovement <= 0 ? Vector2.left : Vector2.right;
        if (grounded)
        {
            rb.AddForce(horizontalDirection * horizontalSpeed);
        }
        else
        {
            rb.AddForce(horizontalDirection * (horizontalSpeed / 2));
        }
    }

    void dashPlayer(int dashingDirection)
    {
        canDash = false;
        playerAnimator.SetTrigger("dashed");
        var dashingPosition = dashingDirection == 0 ? Vector2.left : Vector2.right;
        rb.AddForce(dashSpeed * dashingPosition, ForceMode2D.Impulse);
        StartCoroutine(nameof(dashTimerCoroutine));
    }

    IEnumerator dashTimerCoroutine()
    {
        dashTimer = 0;
        uxInteraction.updatePlayerStamina(0);
        var dashInterval = 1;
        while (enabled)
        {
            dashTimer += Time.deltaTime;

            if (dashTimer >= dashInterval)
            {
                uxInteraction.updatePlayerStamina(dashInterval);
                dashInterval++;
            }

            if (dashTimer >= dashDelay)
            {
                canDash = true;
                StopCoroutine(nameof(dashTimerCoroutine));
            }

            yield return null;
        }
    }

    void swordUpThrust()
    {
        rb.velocity = Vector2.zero;
        jump();
    }

    void jump()
    {
        rb.AddForce(jumpPower * Vector2.up, ForceMode2D.Impulse);
    }

    void climb(float verticalMovement)
    {
        Vector2 verticalDirection =
            verticalMovement <= 0 ? Vector2.down : Vector2.up;
        transform
            .Translate(verticalDirection * Time.deltaTime * climbingSpeed);
    }

    void updatePlayerDirection(float horizontalMovement)
    {
        playerSprite.localScale =
            horizontalMovement >= 0 ? new Vector2(1, 1) : new Vector2(-1, 1);

        isLeft = horizontalMovement >= 0 ? false : true;
    }

    IEnumerator exitLadderCoroutine(Vector3 endPosition)
    {
        while (enabled)
        {
            if (transform.position.y < endPosition.y)
            {
                rb.velocity = new Vector3(isLeft == true ? -1 : 1, 2f, 0) * 3f;
            }
            else
            {
                StopCoroutine("exitLadderCoroutine");
                canMove = true;
            }

            yield return null;
        }
    }

    private void playerHit(int damageHit = 1)
    {
        StartCoroutine(nameof(changeMaterial));
        if (soundManager.fxOn)
        {
            soundManager.playBeenHitSound();
        }

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

        StartCoroutine(nameof(hitDelay));
        Debug.Log("Hit");
    }

    IEnumerator hitDelay()
    {
        canHit = false;
        yield return new WaitForSeconds(1f);
        canHit = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
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
            bool facingRight = rb.velocity.x > 0 ? true : false;
            uxInteraction.enableClimbingIndicator(facingRight);
        }

        if (col.tag == "LadderEnd")
        {
            Debug.Log("Can Climb Jump");
        }

        if (col.tag == "Ladder")
        {
            Debug.Log("Can Climb Ladder");
        }
    }


    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "LadderEnd" && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)))
        {
            Debug.Log("Ladder Jump");

            playerAnimator.SetBool("isClimbing", false);
            canClimb = false;
            canMove = false;
            Vector3 endPosition = transform.position;
            endPosition.y += 2;
            playerAnimator.SetTrigger("climbJump");
            StartCoroutine("exitLadderCoroutine", endPosition);
        }

        if (col.tag == "ScalingWall")
        {
            rb.gravityScale = 0.7f;
            isScalingWall = true;
        }

        if (col.tag == "SideLadder")
        {
            if (horizontalMovement > 0f)
            {
                onSideLadder = true;
                isClimbing = true;
            }
            else
            {
                isClimbing = false;
            }

            if (horizontalMovement < 0)
            {
                onSideLadder = false;
            }
        }

        if (col.tag == "Ladder")
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

        if (col.tag == "ScalingWall")
        {
            rb.gravityScale = 1.0f;
            isScalingWall = false;
        }

        if (col.tag == "SideLadder")
        {
            onSideLadder = false;
            rb.gravityScale = 1;
            playerAnimator.SetBool("onSideLadder", false);
        }

        if (col.tag == "Ladder")
        {
            onLadder = false;
            playerAnimator.SetBool("onLadder", false);
            rb.gravityScale = 1;
            canClimb = false;
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

    public void playerCrushed()
    {
        playerAnimator.SetTrigger("crushed");
        enabled = false;
        uxInteraction.disableUiOnDeath();
        playerStats.CurrentHp = 0;
        uxInteraction.updatePlayerHpBar(0);
        Debug.Log("Player Dead");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
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

        if (col.collider.tag == "ScalingWall")
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

            if (playerVelocity.y < -8.5f)
            {
                playerCrushed();
                Debug.Log("Death by Fall Damage");
            }
        }
    }

    IEnumerator changeMaterial()
    {
        renderer.material = materials[1];
        yield return new WaitForSeconds(0.2f);
        renderer.material = materials[0];
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground" ||
            col.otherCollider.CompareTag("Ground"))
        {
            grounded = false;
            StartCoroutine(nameof(inAirCoroutine));
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
}