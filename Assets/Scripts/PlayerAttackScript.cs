using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    public bool isStabbing = false;
    public bool isDefending = false;

    public bool isParrying = false;
    private Animator animator;

    private PlayerMovement playerMovement;

    public bool playerAction;

    private Rigidbody2D rb;

    [SerializeField]
    private float swordDownThrustPower = 50f;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.S))
        {
            playerAction = true;
            rb.AddForce(Vector2.down * swordDownThrustPower, ForceMode2D.Impulse);
            animator.SetBool("downThrust", true);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && playerMovement.grounded)
        {
            playerAction = true;
            isStabbing = true;
            animator.SetBool("swordPulledBack", true);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && playerMovement.grounded && !playerMovement.canClimb)
        {
            playerAction = true;
            isDefending = true;
            animator.SetBool("isDefending", true);
            StopCoroutine("playerActionWaitTimer");
        }

        if (Input.GetKeyUp(KeyCode.Mouse1) && playerMovement.grounded && !playerMovement.canClimb)
        {
            isDefending = false;
            animator.SetBool("isDefending", false);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0) && playerMovement.grounded)
        {
            isStabbing = false;
            animator.SetBool("downThrust", false);
            animator.SetBool("swordPulledBack", false);
        }
    }

    public void parryAttack()
    {
        isDefending = false;
        animator.SetBool("isDefending", false);
        animator.SetTrigger("isRiposting");
    }
}
