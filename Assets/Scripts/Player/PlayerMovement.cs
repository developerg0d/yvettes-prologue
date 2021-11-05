using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float horizontalSpeed;
    [SerializeField] private float climbingSpeed;
    [SerializeField] private float dashSpeed = 50f;
    [SerializeField] private float jumpPower = 30f;
    private Animator playerAnimator;

    [SerializeField] private Transform playerSprite;

    float horizontalMovement;
    private PlayerAttackScript playerAttackScript;

    private bool moving;

    public bool grounded;
    private Rigidbody2D rb;

    private bool canTurn;
    private bool canDash = true;
    [SerializeField] float dashDelay = 5.0f;

    public GameObject world;

    public bool canClimb;

    private bool upThrustReady;

    private bool isScalingWall;

    private PlayerStats playerStats;

    public bool isLeft;

    private bool onLadder;

    private float exitLadderTimer;

    private bool isJumpingUpLadder;

    public Vector2 playerVelocity;

    private bool isClimbing;
    public bool canMove = true;
    bool onSideLadder;
    public UxInteraction uxInteraction;

    void Start()
    {
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
        if (canMove == true)
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

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) && canClimb)
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
        Vector2 dashingPosition = dashingDirection == 0 ? Vector2.left : Vector2.right;
        rb.AddForce(dashSpeed * dashingPosition, ForceMode2D.Impulse);
        StartCoroutine("dashTimer");
    }

    IEnumerator dashTimer()
    {
        yield return new WaitForSeconds(dashDelay);
        canDash = true;
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

    void OnTriggerEnter2D(Collider2D col)
    {
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
        if (col.tag == "LadderEnd" && isClimbing && canMove)
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
            if (horizontalMovement > 0)
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
            if (!grounded)
            {
                onLadder = true;
            }

            if (rb.gravityScale != 0)
            {
                rb.gravityScale = 0;
            }

            playerAnimator.SetBool("onLadder", true);
            canClimb = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
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
        if (col.gameObject.tag == "Ground" && !isOnLadder() && !isScalingWall)
        {
            rb.gravityScale = 1f;
            upThrustReady = true;
            grounded = true;
        }
    }

    void playerCrushed()
    {
        playerAnimator.SetTrigger("crushed");
        enabled = false;
        uxInteraction.disableUiOnDeath();
        Debug.Log("Player Dead");
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("GolemHand"))
        {
            InteractionGolemHand golemHandScript = col.gameObject.GetComponent<InteractionGolemHand>();

            if (golemHandScript.IsSlamming && playerStats.CanDie)
            {
                playerCrushed();
            }
        }

        if (col.collider.CompareTag("Golem") &&
            col.gameObject.GetComponent<GolemAttackController>().isFalling == true)
        {
            rb.AddForce(Vector2.left * 200);
            playerAttackScript.mediumCameraShake();
            playerCrushed();
        }

        if (col.collider.tag == "ScalingWall")
        {
            rb.velocity = rb.velocity / 4;
        }

        if (col.gameObject.CompareTag("Ground") && !isOnLadder() && !isScalingWall)
        {
            Debug.Log(playerVelocity);
            StopCoroutine(nameof(inAirCoroutine));
            if (!playerStats.CanDie)
            {
                return;
            }

            if (playerVelocity.y < -8.5f)
            {
                playerAnimator.SetTrigger("crushed");
                enabled = false;
                Debug.Log("Death by Fall Damage");
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground")
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