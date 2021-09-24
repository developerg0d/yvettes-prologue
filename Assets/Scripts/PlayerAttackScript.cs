using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackScript : MonoBehaviour
{
    private bool isStabbing = false;
    private bool isDefending = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetBool("swordPulledBack", true);
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            animator.SetBool("swordPulledBack", false);
        }
    }
}
