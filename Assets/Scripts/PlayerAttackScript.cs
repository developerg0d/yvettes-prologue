using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    public bool isStabbing = false;
    public bool isDefending = false;
    private Animator animator;

    public bool playerAction;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            playerAction = true;
            isStabbing = true;
            animator.SetBool("swordPulledBack", true);
            StopCoroutine("playerActionWaitTimer");
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            playerAction = true;
            isDefending = true;
            animator.SetBool("isDefending", true);
            StopCoroutine("playerActionWaitTimer");
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            isDefending = false;
            StartCoroutine("playerActionWaitTimer");
            animator.SetBool("isDefending", false);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            isStabbing = false;
            StartCoroutine("playerActionWaitTimer");
            animator.SetBool("swordPulledBack", false);
        }
    }

    IEnumerator playerActionWaitTimer()
    {
        yield return new WaitForSeconds(1f);
        if (isDefending | isStabbing)
        {
            StopCoroutine("playerActionWaitTimer");
        }
        playerAction = false;
    }
}
