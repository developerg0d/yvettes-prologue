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
    [SerializeField] private Transform playerSprite;

    private PlayerAttackScript playerAttackScript;

    public float horizontalMovement;

    private PlayerInteraction playerInteraction;
    public float stepDelay;
    private bool moving;
    private Rigidbody2D rb;
    private bool isStepping;
    private float dashTimer;
    private bool canTurn;
    private bool canDash = true;
    [SerializeField] float dashDelay = 3.0f;

    public GameObject world;


    public bool isLeft;

    private bool isClimbing;
    private float exitLadderTimer;

    private bool isJumpingUpLadder;

    public bool canMove = true;
    bool onSideLadder;
    public UxInteraction uxInteraction;
    public GameObject playerSpawnEffect;
    private SoundManager soundManager;

    void Awake()
    {
        StartCoroutine(nameof(stepping));
        soundManager = GameObject.FindWithTag("AudioSource").GetComponent<AudioSource>().GetComponent<SoundManager>();
        spawnEffect();
    }

    public void spawnEffect()
    {
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
        playerInteraction = GetComponent<PlayerInteraction>();
        playerAttackScript = GetComponent<PlayerAttackScript>();
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        playerAnimator.SetBool("isScalingWall", playerInteraction.isScalingWall);
        playerAnimator.SetBool("onSideLadder", onSideLadder);
        playerAnimator.SetBool("isSideLadderClimbing", isClimbing);
        playerAnimator.SetBool("isMoving", moving);
        playerAnimator.SetBool("onGround", playerInteraction.grounded);
    }

    void Update()
    {
        if (canMove)
        {
            MovementControls();
        }
    }

    void MovementControls()
    {
        if (canMove && Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.W) && !playerInteraction.grounded &&
            playerInteraction.upThrustReady)
        {
            playerInteraction.upThrustReady = false;
            playerAnimator.SetBool("upthrust", true);
            swordUpThrust();
        }

        if (canMove && Input.GetKeyUp(KeyCode.Mouse0))
        {
            playerAnimator.SetBool("upthrust", false);
        }

        if (canMove && Input.GetKeyDown(KeyCode.Q) && !playerAttackScript.isDefending && canDash)
        {
            float dashingAnimationDirection = isLeft == true ? 1 : 0;
            playerAnimator.SetFloat("dashDirection", dashingAnimationDirection);
            dashPlayer(0);
        }
        else if (canMove && Input.GetKeyDown(KeyCode.E) && !playerAttackScript.isDefending && canDash)
        {
            float dashingAnimationDirection = isLeft == true ? 0 : 1;
            playerAnimator.SetFloat("dashDirection", dashingAnimationDirection);
            dashPlayer(1);
        }

        if ((canMove && Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S)) && playerInteraction.canClimb)
        {
            isClimbing = true;
            playerAnimator.SetBool("isClimbing", true);
        }

        if (canMove && Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) && playerInteraction.canClimb)
        {
            isClimbing = false;
            playerAnimator.SetBool("isClimbing", false);
        }

        if (canMove && Input.GetKeyDown(KeyCode.Space) && playerInteraction.grounded)
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
        if (playerInteraction.canClimb && verticalMovement != 0)
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

    IEnumerator stepping()
    {
        while (enabled)
        {
            if (moving && playerInteraction.grounded && canMove)
            {
                if (soundManager.fxOn)
                {
                    soundManager.playStepSound();
                }

                yield return new WaitForSeconds(stepDelay);
            }

            yield return null;
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
        if (playerInteraction.grounded)
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
}