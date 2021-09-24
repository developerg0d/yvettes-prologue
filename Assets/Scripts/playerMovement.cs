using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float horizontalSpeed;
    [SerializeField]
    private float dashSpeed = 10;
    private Animator playerAnimator;

    [SerializeField]
    private Transform playerSprite;

    [SerializeField]
    private Transform bottomPlayerSprite;

    private PlayerAttackScript playerAttackScript;

    private bool moving;

    private Rigidbody2D rb;

    private bool canTurn;

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

    }

    void FixedUpdate()
    {
        float horizontalMovement = Input.GetAxis("HorizontalMoving");
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

}
