using System;
using System.Collections;
using UnityEngine;

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

    private CameraShake cameraShake;

    public Material[] materials;

    private PlayerMovement playerMovement;

    private void Start()
    {
        assignVariables();
    }

    private void assignVariables()
    {
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
            if (playerMovement.horizontalMovement > 0f)
            {
                onSideLadder = true;
            }

            if (playerMovement.horizontalMovement < 0)
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

    bool isOnLadder()
    {
        return onLadder || onSideLadder;
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
                StopCoroutine("exitLadderCoroutine");
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
        if (soundManager.fxOn)
        {
            soundManager.playHitSound();
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


    public void playerCrushed()
    {
        playerAnimator.SetTrigger("crushed");
        enabled = false;
        uxInteraction.disableUiOnDeath();
        playerStats.CurrentHp = 0;
        uxInteraction.updatePlayerHpBar(0);
        Debug.Log("Player Dead");
    }


    IEnumerator changeMaterial()
    {
        GetComponent<Renderer>().material = materials[1];
        yield return new WaitForSeconds(0.2f);
        GetComponent<Renderer>().material = materials[0];
    }
}