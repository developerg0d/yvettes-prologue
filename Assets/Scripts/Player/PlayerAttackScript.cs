using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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

    [SerializeField] private float swordDownThrustPower = 50f;

    public CameraShake cameraShake;
    public CinemachineVirtualCamera virtualCamera;

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
            rb.AddForce(Vector2.down * swordDownThrustPower, ForceMode2D.Impulse);
            animator.SetBool("downThrust", true);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) &&
            playerMovement.grounded)
        {
            isStabbing = true;

            animator.SetBool("swordPulledBack", true);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && playerMovement.grounded && !playerMovement.canClimb)
        {
            isDefending = true;
            animator.SetBool("isDefending", true);
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

    public void playerDied()
    {
        Debug.Log("Player Died");
        cameraShake.shakeCamera(0.4f, 0.2f);
        animator.SetTrigger("crushed");
        enabled = false;
    }

    public void playerParried()
    {
        Debug.Log("Player Parried");
        StartCoroutine(nameof(majorImpactAction));
    }

    public void playerDefended()
    {
        Debug.Log("Player Defended");
        mediumCameraShake();
    }

    public void mediumCameraShake()
    {
        cameraShake.shakeCamera(0.2f, 0.2f);
    }

    IEnumerator majorImpactAction()
    {
        Time.timeScale = 0.1f;
        virtualCamera.m_Lens.OrthographicSize = 5;
        virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 1.75f;
        yield return new WaitForSeconds(0.1f);
        cameraShake.shakeCamera(0.5f, 0.2f);
        virtualCamera.m_Lens.OrthographicSize = 10;
        virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 3f;
        Time.timeScale = 1f;
        GetComponent<Animator>().SetTrigger("parried");
        isDefending = false;
    }
}