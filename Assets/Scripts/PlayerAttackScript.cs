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
            animator.SetBool("swordPulledBack", true);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StartCoroutine("playerActionWaitTimer");
            animator.SetBool("swordPulledBack", false);
        }
    }

    IEnumerator playerActionWaitTimer()
    {
        yield return new WaitForSeconds(1f);
        playerAction = false;
    }
}
