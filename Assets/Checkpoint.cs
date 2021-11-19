using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool activated;

    public Sprite activatedSprite;
    public GameObject[] checkpoints;

    void Start()
    {
        checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") && !activated)
        {
            activated = true;
            GetComponent<SpriteRenderer>().sprite = activatedSprite;
        }
    }
}