using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float horizontalSpeed;
    [SerializeField]
    private float climbingSpeed;
    [SerializeField]
    private float dashSpeed = 50f;
    [SerializeField]
    private float jumpPower = 30f;
    private Animator playerAnimator;

    [SerializeField]
    private Transform playerSprite;

    [SerializeField]
    private Transform bottomPlayerSprite;

    private PlayerAttackScript playerAttackScript;

    private bool moving;

    private bool grounded;
    private Rigidbody2D rb;

    private bool canTurn;

    public GameObject world;

    [SerializeField] private bool canClimb;

    void Start()
    {
        playerAttackScript = GetComponent<PlayerAttackScript>();
        playerAnimator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        playerAnimator.SetBool("isMoving", moving);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            dashPlayer(0);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            dashPlayer(1);
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            jump();
        }
    }

    void FixedUpdate()
    {
        float horizontalMovement = Input.GetAxis("HorizontalMoving");
        float verticalMovement = Input.GetAxis("VerticalMoving");
        if (canClimb && verticalMovement != 0)
        {
            climb(verticalMovement);
        }
        if (horizontalMovement != 0)
        {
            movePlayerHorizontally(horizontalMovement);
        }
        isPlayerMoving(horizontalMovement);
    }

    void isPlayerMoving(float horizontalMovement)
    {
        moving = horizontalMovement != 0;
    }

    void dashPlayer(int dashingDirection)
    {
        Vector2 dashingPosition = dashingDirection == 0 ? Vector2.left : Vector2.right;
        rb.AddForce(dashSpeed * dashingPosition, ForceMode2D.Impulse);
    }

    void jump()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(jumpPower * Vector2.up, ForceMode2D.Impulse);
    }


    void climb(float verticalMovement)
    {
        Vector2 verticalDirection =
            verticalMovement <= 0 ? Vector2.down : Vector2.up;
        transform
            .Translate(verticalDirection * Time.deltaTime * climbingSpeed);
    }
    void movePlayerHorizontally(float horizontalMovement)
    {
        float currentHorizontalSpeed = playerAttackScript.isStabbing == true | playerAttackScript.isDefending == true ? horizontalSpeed / 2 : horizontalSpeed;
        updatePlayerDirection(horizontalMovement);

        Vector2 horizontalDirection =
            horizontalMovement <= 0 ? Vector2.left : Vector2.right;
        transform
            .Translate(horizontalDirection * Time.deltaTime * currentHorizontalSpeed);
    }

    void updatePlayerDirection(float horizontalMovement)
    {
        if (playerAttackScript.playerAction | playerAttackScript.isStabbing | playerAttackScript.isDefending)
        {
            return;
        }

        playerSprite.localScale =
            horizontalMovement >= 0 ? new Vector2(1, 1) : new Vector2(-1, 1);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Ladder")
        {
            rb.gravityScale = 0;
            canClimb = true;
        }
    }
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Ladder")
        {
            rb.gravityScale = 1;
            canClimb = false;
        }
    }
    void OnCollisionStay2D(Collision2D col)
    {
        if (col.collider.tag == "ScalingWall")
        {
            Debug.Log("Test");
            rb.gravityScale = 0.3f;
            playerAnimator.SetBool("isScalingWall", true);
        }

        if (col.gameObject.layer == 0)
        {
            grounded = true;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {


        if (col.collider.tag == "GolemHand")
        {
            col.gameObject.GetComponentInParent<AttackControllerTheGolem>().playerRidingHand = true;
            transform.SetParent(col.gameObject.transform);
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {

        if (col.collider.tag == "GolemHand")
        {
            col.gameObject.GetComponentInParent<AttackControllerTheGolem>().playerRidingHand = false;
            transform.SetParent(world.transform);
        }
        if (col.gameObject.layer == 0)
        {
            grounded = false;
        }
        if (col.collider.tag == "ScalingWall")
        {
            rb.gravityScale = 1.0f;
            playerAnimator.SetBool("isScalingWall", false);
        }
    }
}
